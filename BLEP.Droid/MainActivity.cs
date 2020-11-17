using System;
using System.Collections.Generic;
using System.Linq;
using Android;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using BLEP.Droid.BlueT;
using BLEP.Droid.ListView;

namespace BLEP.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {


        private const int LocationPermissionsRequestCode = 1000;

        private static readonly string[] LocationPermissions =
        {
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation
        };

        private static MainActivity _instance;
        private bool _isReceiveredRegistered;
        private BluetoothDeviceReceiver _receiver;

        public static MainActivity GetInstance()
        {
            return _instance;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;


            _instance = this;

            SetContentView(Resource.Layout.activity_main);

            var coarseLocationPermissionGranted =
                ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation);
            var fineLocationPermissionGranted =
                ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation);

            if (coarseLocationPermissionGranted != Permission.Denied ||
                fineLocationPermissionGranted == Permission.Denied)
                ActivityCompat.RequestPermissions(this, LocationPermissions, LocationPermissionsRequestCode);

            // Register for broadcasts when a device is discovered
            _receiver = new BluetoothDeviceReceiver();

            RegisterBluetoothReceiver();

            PopulateListView();
        }

       
        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Make sure we're not doing discovery anymore
            CancelScanning();

            // Unregister broadcast listeners
            UnregisterBluetoothReceiver();
        }

        protected override void OnPause()
        {
            base.OnPause();

            // Make sure we're not doing discovery anymore
            CancelScanning();

            // Unregister broadcast listeners
            UnregisterBluetoothReceiver();
        }

        protected override void OnResume()
        {
            base.OnResume();

            StartScanning();

            // Register broadcast listeners
            RegisterBluetoothReceiver();
        }

        public void UpdateAdapter(DataItem dataItem)
        {
            var lst = FindViewById<Android.Widget.ListView>(Resource.Id.lstview);
            var adapter = lst.Adapter as ListViewAdapter;

            var items = adapter?.Items.Where(m => m.GetListItemType() == ListItemType.DataItem).ToList();

            if (items != null && !items.Any(x =>
                    ((DataItem)x).Text == dataItem.Text && ((DataItem)x).SubTitle == dataItem.SubTitle))
            {
                adapter.Items.Add(dataItem);
            }

            lst.Adapter = new ListViewAdapter(this, adapter?.Items);
        }

        private static void StartScanning()
        {
            if (!BluetoothDeviceReceiver.Adapter.IsDiscovering) BluetoothDeviceReceiver.Adapter.StartDiscovery();
        }

        private static void CancelScanning()
        {
            if (BluetoothDeviceReceiver.Adapter.IsDiscovering) BluetoothDeviceReceiver.Adapter.CancelDiscovery();
        }

        private void RegisterBluetoothReceiver()
        {
            if (_isReceiveredRegistered) return;

            RegisterReceiver(_receiver, new IntentFilter(BluetoothDevice.ActionFound));
            RegisterReceiver(_receiver, new IntentFilter(BluetoothAdapter.ActionDiscoveryStarted));
            RegisterReceiver(_receiver, new IntentFilter(BluetoothAdapter.ActionDiscoveryFinished));
            _isReceiveredRegistered = true;
        }

        private void UnregisterBluetoothReceiver()
        {
            if (!_isReceiveredRegistered) return;

            UnregisterReceiver(_receiver);
            _isReceiveredRegistered = false;
        }

        private void PopulateListView()
        {
            var item = new List<IListItem>
            {
                new HeaderListItem("PREVIOUSLY PAIRED")
            };

            item.AddRange(
                BluetoothDeviceReceiver.Adapter.BondedDevices.Select(
                    bluetoothDevice => new DataItem(
                        bluetoothDevice.Name,
                        bluetoothDevice.Address
                    )
                )
            );

            StartScanning();

            item.Add(new StatusHeaderListItem("Scanning started..."));

            var lst = FindViewById<Android.Widget.ListView>(Resource.Id.lstview);
            lst.Adapter = new ListViewAdapter(this, item);
        }

        public void UpdateAdapterStatus(string discoveryStatus)
        {
            var lst = FindViewById<Android.Widget.ListView>(Resource.Id.lstview);
            var adapter = lst.Adapter as ListViewAdapter;

            var hasStatusItem = adapter?.Items?.Any(m => m.GetListItemType() == ListItemType.Status);

            if (hasStatusItem.HasValue && hasStatusItem.Value)
            {
                var statusItem = adapter.Items.Single(m => m.GetListItemType() == ListItemType.Status);
                statusItem.Text = discoveryStatus;
            }

            lst.Adapter = new ListViewAdapter(this, adapter?.Items);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
