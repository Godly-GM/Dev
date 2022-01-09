#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppExtensions;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Media.Imaging;

namespace DevToys.Core
{
    internal sealed class ExtensionManager
    {
        private const string ExtensionContractName = "com.devtoys.tool";

        private readonly AppExtensionCatalog _catalog;

        public ObservableCollection<Extension> Extensions { get; } = new ObservableCollection<Extension>();

        public ExtensionManager()
        {
            _catalog = AppExtensionCatalog.Open(ExtensionContractName);
            _catalog.PackageInstalled += Catalog_PackageInstalled;
            _catalog.PackageUpdated += Catalog_PackageUpdated;
            _catalog.PackageUninstalling += Catalog_PackageUninstalling;
            _catalog.PackageUpdating += Catalog_PackageUpdating;
            _catalog.PackageStatusChanged += Catalog_PackageStatusChanged;

            FindAndLoadExtensions();
        }

        private void Catalog_PackageInstalled(AppExtensionCatalog sender, AppExtensionPackageInstalledEventArgs args)
        {
        }

        private void Catalog_PackageUpdated(AppExtensionCatalog sender, AppExtensionPackageUpdatedEventArgs args)
        {
        }

        private void Catalog_PackageUninstalling(AppExtensionCatalog sender, AppExtensionPackageUninstallingEventArgs args)
        {
        }

        private void Catalog_PackageUpdating(AppExtensionCatalog sender, AppExtensionPackageUpdatingEventArgs args)
        {
        }

        private void Catalog_PackageStatusChanged(AppExtensionCatalog sender, AppExtensionPackageStatusChangedEventArgs args)
        {
        }

        private async void FindAndLoadExtensions()
        {
            IReadOnlyList<AppExtension> extensions = await _catalog.FindAllAsync();
            foreach (AppExtension extension in extensions)
            {
                await LoadExtension(extension);
            }
        }

        private async Task LoadExtension(AppExtension ext)
        {
            // Build a unique identifier for this extension
            string identifier = ext.AppInfo.AppUserModelId + "!" + ext.Id;

            // load the extension if the package is OK
            if (!ext.Package.Status.VerifyIsOK()
                /* This is a good place to do package signature verfication
                   For the purpose of the sample, we ignore where the package is from
                   Here is an example of how you would ensure that you only load store-signed extensions:

                    && ext.Package.SignatureKind == PackageSignatureKind.Store */
                )
            {
                return; // Because this package doesn't meet our requirements, don't load it
            }

            // if we already have an extension by this name then then this load is really an update to the extension
            Extension existingExt = Extensions.Where(e => e.UniqueId == identifier).FirstOrDefault();

            // New extension?
            if (existingExt == null)
            {
                // get the extension's properties, such as its logo
                var properties = (PropertySet)await ext.GetExtensionPropertiesAsync();
                var filestream = await (ext.AppInfo.DisplayInfo.GetLogo(new Windows.Foundation.Size(1, 1))).OpenReadAsync();
                BitmapImage logo = new BitmapImage();
                logo.SetSource(filestream);

                Extension newExtension = new Extension(ext, properties, logo);
                Extensions.Add(newExtension);

                await newExtension.MarkAsLoaded();
            }
            else // update scenario
            {
                // unload the old version of the extension first
                existingExt.Unload();

                // update the extension
                await existingExt.Update(ext);
            }
        }
    }
}
