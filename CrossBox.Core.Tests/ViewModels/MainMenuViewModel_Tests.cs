﻿using System;
using System.Linq;
using System.Text;
using CrossBox.Core.DropBox;
using CrossBox.Core.Services;
using CrossBox.Core.Tests.Mocks;
using CrossBox.Core.ViewModels;
using NUnit.Framework;

namespace CrossBox.Core.Tests.ViewModels
{
    [TestFixture]
    public class MainMenuViewModel_Tests
    {

        private readonly DropBoxItem[] _rootFolderContents = new DropBoxItem[]
        {
            new DropBoxFolder("folder", "folder"),
            new DropBoxFile("file.txt", "file.txt")
        };

        private readonly DropBoxItem[] _childFolderContents = new DropBoxItem[]
        {
            new DropBoxFile("folder/file1.txt", "file1.txt"),
            new DropBoxFile("folder/file2.txt", "file2.txt"),
            new DropBoxFile("folder/file3.txt", "file3.txt")
        };

        private MockSetup _setup;
        private IDropBoxClient _client;
        private IFileSelector _fileSelector = new FileSelectorMock(null);

        public void SetUp_To_Return_FolderContent()
        {
            _client = new DropBoxClientMock_ReturnsFolderContent(
                folder =>
                {
                    if (folder == "" || folder == "/")
                    {
                        return _rootFolderContents;
                    }
                    if (string.Equals(folder, "folder", StringComparison.OrdinalIgnoreCase))
                    {
                        return _childFolderContents;
                    }
                    return new DropBoxItem[0];
                });
            _setup = new MockSetup(_client, _fileSelector);
            _setup.Initialize();
        }

        public void SetUp_To_Upload_Selected_File(SelectedFile selectedFile)
        {
            _fileSelector = new FileSelectorMock(selectedFile);
            _client = new DropBoxClientMock_StoresUploadedFile();
            _setup = new MockSetup(_client, _fileSelector);
            _setup.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            if (_setup != null)
            {
                _setup.Dispose();
            }
        }

        [Test]
        public void Assure_Content_List_Has_Expected_Number_Of_Items()
        {
            SetUp_To_Return_FolderContent();
            var viewModel = new MainMenuViewModel();
            viewModel.SelectFolder("/", () =>
                Assert.That(viewModel.FolderContents, Has.Count.EqualTo(_rootFolderContents.Length)));
        }

        [Test]
        public void Assure_Content_List_Has_Expected_Number_Of_Files()
        {
            SetUp_To_Return_FolderContent();
            var viewModel = new MainMenuViewModel();
            viewModel.SelectFolder("/", () =>
                Assert.That(viewModel.FolderContents.Where(c => !c.IsDirectory).SingleOrDefault(), Is.Not.Null));
        }

        [Test]
        public void Assure_Content_List_Has_Directory_With_Expected_Properties()
        {
            SetUp_To_Return_FolderContent();
            var expectedFolder = _rootFolderContents.OfType<DropBoxFolder>().First();

            var viewModel = new MainMenuViewModel();
            viewModel.SelectFolder("/", () =>
            {
                var folder = viewModel.FolderContents.Where(c => c.IsDirectory).Single();
                Assert.That(folder.Name, Is.EqualTo(expectedFolder.Name));
                Assert.That(folder.FullPath, Is.EqualTo(expectedFolder.FullPath));
            });

        }

        [Test]
        public void Assure_DropBox_Errors_Are_Reported()
        {
            Exception reportedException = null;
            Setup_With_GetFolderContent_Error(exception => reportedException = exception);

            var viewModel = new MainMenuViewModel();
            viewModel.SelectFolder("", () =>
                Assert.That(reportedException, Is.Not.Null));
        }

        private void Setup_With_GetFolderContent_Error(Action<Exception> onError)
        {
            _client = new DropBoxClientMock_FailsOnGetFolderContent("Error");
            _setup = new MockSetup(_client, _fileSelector, onError);
            _setup.Initialize();
        }

        [Test]
        public void Assure_Failed_DropBox_Authentication_Is_Reported_As_Error()
        {
            Exception reportedException = null;
            Setup_With_EnsureIsAuthenticated_Error(exception => reportedException = exception);

            var viewModel = new MainMenuViewModel();
            viewModel.SelectFolder("", () =>
                Assert.That(reportedException, Is.Not.Null));
        }

        private void Setup_With_EnsureIsAuthenticated_Error(Action<Exception> onError)
        {
            _client = new DropBoxClientMock_FailsOnEnsureIsAuthenticated("Error");
            _setup = new MockSetup(_client, _fileSelector, onError);
            _setup.Initialize();
        }

        [Test]
        public void Assure_Client_Is_Authenticated_When_Loading_A_Folder()
        {
            SetUp_To_Return_FolderContent();
            var client = (DropBoxClientMock_ReturnsFolderContent)_client;

            var viewModel = new MainMenuViewModel();
            viewModel.SelectFolder("", () =>
                Assert.That(client.EnsureIsAuthenticatedWasRun, Is.True));

        }

        [Test]
        public void Assure_Folder_Selection_Causes_FolderContents_To_Be_Updated()
        {
            SetUp_To_Return_FolderContent();

            var viewModel = new MainMenuViewModel();
            viewModel.SelectFolder("/", () =>
                viewModel.SelectFolder("folder", () =>
                {
                    Assert.That(viewModel.FolderContents, Has.Count.EqualTo(_childFolderContents.Length));

                    _childFolderContents.ToList()
                        .ForEach(content =>
                                Assert.That(viewModel.FolderContents.Single(fc => fc.FullPath.Equals(content.FullPath)), Is.Not.Null));
                }));
        }
        
        [Test]
        public void Assure_SelectItemCommand_Navigates_To_Selected_Folder()
        {
            SetUp_To_Return_FolderContent();

            var viewModel = new MainMenuViewModel();
            viewModel.SelectFolder("",
                () =>
                {
                    var folder = viewModel.FolderContents.First(fc => fc.IsDirectory);
                    viewModel.SelectItemCommand.Execute(folder);

                    var request = _setup.Dispatcher.Requests.Single();
                    Assert.That(request.ParameterValues.Single().Value, Is.EqualTo(folder.Name));
                });
        }

        [Test]
        public void Assure_Folder_Selection_Updates_FolderName_Property()
        {
            SetUp_To_Return_FolderContent();

            var viewModel = new MainMenuViewModel();
            viewModel.SelectFolder("folder", () =>
                Assert.That(viewModel.FolderName, Is.EqualTo("folder")));
        }

        [Test]
        public void Assure_Folder_Selection_Causes_FolderName_PropertyChanged_Notification()
        {
            SetUp_To_Return_FolderContent();

            var propertyChanged = "";

            var viewModel = new MainMenuViewModel();
            viewModel.PropertyChanged += (s, e) => { propertyChanged += e.PropertyName + ","; };

            viewModel.SelectFolder("folder", () =>
                Assert.That(propertyChanged, Contains.Substring("FolderName")));
        }

        [Test]
        public void Assure_File_Selection_Causes_Navigate_To_FileContentViewModel()
        {
            SetUp_To_Return_FolderContent();

            var viewModel = new MainMenuViewModel();
            viewModel.SelectFolder("",
                () =>
                {
                    var file = viewModel.FolderContents.First(fc => !fc.IsDirectory);
                    viewModel.SelectItemCommand.Execute(file);
                    var request = _setup.Dispatcher.Requests.Single();
                    
                    Assert.That(request.ViewModelType, Is.EqualTo(typeof(FileContentViewModel)));
                });
        }

        [Test]
        public void Assure_ViewModel_Created_Using_Default_Constructor_Navigates_To_Root_Folder()
        {
            SetUp_To_Return_FolderContent();

            var viewModel = new MainMenuViewModel();
            Assert.That(viewModel.FolderName, Is.EqualTo("/"));
        }

        [Test]
        public void Assure_ViewModel_Created_With_Folder_Argument_Navigates_To_Named_Folder()
        {
            SetUp_To_Return_FolderContent();

            const string folderName = "folder";
            var viewModel = new MainMenuViewModel(folderName);
            Assert.That(viewModel.FolderName, Is.EqualTo(folderName));
        }

        [Test]
        public void Assure_UploadFileCommand_Causes_File_Selector_To_Uploaded()
        {
            var fileBytes = Encoding.UTF8.GetBytes("hello world!");
            const string fileName = "file name";
            var file = new SelectedFile(fileName, fileBytes);

            SetUp_To_Upload_Selected_File(file);


            var viewModel = new MainMenuViewModel();
            viewModel.UploadFileCommand.Execute();

            var mock = (DropBoxClientMock_StoresUploadedFile) _client;
            Assert.That(mock.UploadedFile.Name, Is.EqualTo(file.FileName));

        }
    }
}
