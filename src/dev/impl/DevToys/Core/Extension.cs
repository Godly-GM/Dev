#nullable enable

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Windows.ApplicationModel.AppExtensions;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace DevToys.Core
{
    /// <summary>
    /// Represents an extension in the ExtensionManager
    /// </summary>
    internal sealed class Extension : ObservableRecipient
    {
        #region Member Vars
        private PropertySet _properties;
        private string _serviceName;
        private readonly object _sync = new object();
        private AppServiceConnection connection;
        #endregion

        /// <summary>
        /// Creates an Extension object that represents an extension in the extension manager
        /// </summary>
        /// <param name="ext">The extension as represented by the system</param>
        /// <param name="properties">Properties about the extension</param>
        /// <param name="logo">The logo associated with the package that the extension is defined in</param>
        public Extension(AppExtension ext, PropertySet properties, BitmapImage logo)
        {
            AppExtension = ext;
            _properties = properties;
            Enabled = false;
            Loaded = false;
            Offline = false;
            Logo = logo;
            Visible = Visibility.Collapsed;

            #region Properties
            _serviceName = null;
            if (_properties != null)
            {
                if (_properties.ContainsKey("Service"))
                {
                    PropertySet serviceProperty = (PropertySet)_properties["Service"];
                    _serviceName = serviceProperty["#text"].ToString();
                }
            }
            #endregion

            UniqueId = ext.AppInfo.AppUserModelId + "!" + ext.Id; // The name that identifies this extension in the extension manager
        }

        #region Properties
        public BitmapImage Logo { get; private set; }

        public string UniqueId { get; private set; } // the unique id of this extension which will be AppUserModel Id + Extension ID

        public bool Enabled { get; private set; } // whether the user has enabled the extension or not

        public bool Offline { get; private set; } // whether the package containing the extension is offline

        public bool Loaded { get; private set; } // whether the package has been loaded or not.

        public AppExtension AppExtension { get; private set; }

        public Visibility Visible { get; private set; } // Whether the extension should be visible in the list of extensions
        #endregion

        /// <summary>
        /// Invoke the extension's app service
        /// </summary>
        /// <param name="message">The parameters for the app service call</param>
        public async Task<double> Invoke(ValueSet message)
        {
            if (Loaded)
            {
                try
                {
                    // make the app service call
                    //using (var connection = new AppServiceConnection())

                    if (connection is null)
                    {
                        connection = connection ?? new AppServiceConnection();
                        // service name is defined in appxmanifest properties
                        connection.AppServiceName = _serviceName;
                        // package Family Name is provided by the extension
                        connection.PackageFamilyName = AppExtension.Package.Id.FamilyName;

                        // open the app service connection
                        AppServiceConnectionStatus status = await connection.OpenAsync();
                        if (status != AppServiceConnectionStatus.Success)
                        {
                            Debug.WriteLine("Failed App Service Connection");
                        }
                    }

                    {
                        // Call the app service
                        AppServiceResponse response = await connection.SendMessageAsync(message);
                        if (response.Status == AppServiceResponseStatus.Success)
                        {
                            ValueSet answer = response.Message as ValueSet;
                            if (answer.ContainsKey("Result")) // When our app service returns "Result", it means it succeeded
                            {
                                return (double)answer["Result"];
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    Debug.WriteLine("Calling the App Service failed");
                }
            }
            return double.NaN; // indicates an error from the app service
        }

        /// <summary>
        /// Called when an extension that has already been loaded is updated
        /// </summary>
        /// <param name="ext">The updated extension as represented by the system</param>
        /// <returns></returns>
        public async Task Update(AppExtension ext)
        {
            // ensure this is the same uid
            string identifier = ext.AppInfo.AppUserModelId + "!" + ext.Id;
            if (identifier != this.UniqueId)
            {
                return;
            }

            var properties = await ext.GetExtensionPropertiesAsync() as PropertySet;

            // get the logo for the extension
            var filestream = await (ext.AppInfo.DisplayInfo.GetLogo(new Windows.Foundation.Size(1, 1))).OpenReadAsync();
            BitmapImage logo = new BitmapImage();
            logo.SetSource(filestream);

            // update the extension
            this.AppExtension = ext;
            _properties = properties;
            Logo = logo;

            #region Update Properties
            // update app service information
            _serviceName = null;
            if (_properties != null)
            {
                if (_properties.ContainsKey("Service"))
                {
                    PropertySet serviceProperty = (PropertySet)_properties["Service"];
                    this._serviceName = serviceProperty["#text"].ToString();
                }
            }
            #endregion

            await MarkAsLoaded();
        }

        /// <summary>
        /// Prepares the extension so that the ExtensionManager can present it as an available extension
        /// </summary>
        /// <returns></returns>
        public async Task MarkAsLoaded()
        {
            // make sure package is OK to load
            if (!AppExtension.Package.Status.VerifyIsOK())
            {
                return;
            }

            Enabled = true;

            // Don't reload
            if (Loaded)
            {
                return;
            }

            // The public folder is shared between the extension and the host.
            // We don't use it in this sample but you can see https://github.com/Microsoft/Build2016-B808-AppExtensibilitySample for an example of it can be used.
            StorageFolder folder = await AppExtension.GetPublicFolderAsync();

            Loaded = true;
            Visible = Visibility.Visible;
            OnPropertyChanged(nameof(Visible));
            Offline = false;
        }

        /// <summary>
        /// Enable the extension for use
        /// </summary>
        /// <returns></returns>
        public async Task Enable()
        {
            Enabled = true;
            await MarkAsLoaded();
        }

        /// <summary>
        /// Indicates to the extension manager that the extension is unloaded
        /// </summary>
        public void Unload()
        {
            // unload it
            lock (_sync) // Calls to this functioned are queued on an await call so lock to handle one at a time
            {
                if (Loaded)
                {
                    // see if the package is offline
                    if (!AppExtension.Package.Status.VerifyIsOK() && !AppExtension.Package.Status.PackageOffline)
                    {
                        Offline = true;
                    }

                    Loaded = false;
                    Visible = Visibility.Collapsed;
                    OnPropertyChanged(nameof(Visible));
                }
            }
        }

        // user-facing action to disable the extension
        public void Disable()
        {
            // only disable if it is enabled so that we don't Unload() more than once
            if (Enabled)
            {
                Enabled = false;
                Unload();
            }
        }

    }
}
