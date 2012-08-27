﻿using System;
using System.Linq;
using CrossBox.Core.DropBox;
using CrossBox.Core.Tests.Mocks;
using CrossBox.Core.ViewModels;
using NUnit.Framework;
using Cirrious.MvvmCross.ExtensionMethods;

namespace CrossBox.Core.Tests.ViewModels
{
    [TestFixture]
    public class MainMenuViewModel_Tests
    {

        private readonly DropBoxItem[] _rootFolderContents = new DropBoxItem[]
                                              {
                                                  new DropBoxFolder("folder", "folder"),
                                                  new DropBoxFile("file path", "file name.txt")
                                              };

        private readonly DropBoxItem[] _childFolderContents = new DropBoxItem[]
                                                                  {
                                                                      new DropBoxFile("folder/file1.txt", "file1.txt"),
                                                                      new DropBoxFile("folder/file2.txt", "file2.txt"),
                                                                      new DropBoxFile("folder/file3.txt", "file3.txt")
                                                                  };

        private MockSetup _setup;
        private IDropBoxClient _client;


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
            _setup = new MockSetup(_client);
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
<<<<<<< HEAD
            SetUp_To_Return_FolderContent();
            var expectedFolder = _contents.OfType<DropBoxFolder>().First();
=======
            _setup.Initialize();
            var expectedFolder = _rootFolderContents.OfType<DropBoxFolder>().First();
>>>>>>> Refactored dropbox client mock for easier testing

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
            _setup = new MockSetup(_client, onError);
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
            _setup = new MockSetup(_client, onError);
            _setup.Initialize();
        }

<<<<<<< HEAD
<<<<<<< HEAD
        [Test]
        public void Assure_Client_Is_Authenticated_When_Loading_A_Folder()
        {
            SetUp_To_Return_FolderContent();
            var client = (DropBoxClientMock_ReturnsFolderContent) _client;

            var viewModel = new MainMenuViewModel();
            viewModel.SelectFolder("", () => 
                Assert.That(client.EnsureIsAuthenticatedWasRun, Is.True));

        }
=======
>>>>>>> Refactored dropbox client mock for easier testing
=======
        [Test]
        public void Assure_Folder_Selection_Causes_FolderContents_To_Be_Updated()
        {
            _setup.Initialize();

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
        public void Assure_Folder_Selection_Causes_PropertyChanged_Notification()
        {
            _setup.Initialize();

            var propertyChanged = "";

            var viewModel = new MainMenuViewModel();
            viewModel.PropertyChanged += (s, e) => { propertyChanged += e.PropertyName + ","; };

            viewModel.SelectFolder("folder", () => 
                Assert.That(propertyChanged, Contains.Substring("FolderContents")));

        }

>>>>>>> Added update notification when changing folders
    }
}
