﻿using System;
using System.Linq;
using Cirrious.MvvmCross.IoC;
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

        private readonly DropBoxItem[] _contents = new DropBoxItem[]
                                              {
                                                  new DropBoxFolder("folder path", "folder name"),
                                                  new DropBoxFile("file path", "file name")
                                              };

        private MockSetup _setup;
        private IDropBoxClient _client;

        [SetUp]
        public void SetUp()
        {
            _client = new DropBoxClientMock_ReturnsFolderContent(_contents);
            _setup = new MockSetup(_client);
        }

        [TearDown]
        public void TearDown()
        {
            _setup.Dispose();
        }

        [Test]
        public void Assure_Content_List_Has_Expected_Number_Of_Items()
        {
            _setup.Initialize();
            var viewModel = new MainMenuViewModel();
            viewModel.SelectFolder("/", () => 
                Assert.That(viewModel.FolderContents, Has.Count.EqualTo(_contents.Length)));            
        }

        [Test]
        public void Assure_Content_List_Has_Expected_Number_Of_Files()
        {
            _setup.Initialize();
            var viewModel = new MainMenuViewModel();
            viewModel.SelectFolder("/", () => 
                Assert.That(viewModel.FolderContents.Where(c => !c.IsDirectory).SingleOrDefault(), Is.Not.Null));
        }

        [Test]
        public void Assure_Content_List_Has_Directory_With_Expected_Properties()
        {
            _setup.Initialize();
            var expectedFolder = _contents.OfType<DropBoxFolder>().First();

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
    }
}
