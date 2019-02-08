﻿using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Unicord.Universal.Misc;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Unicord.Universal.Pages.Subpages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserListPage : Page
    {
        private DiscordChannel _channel;
        private CancellationTokenSource _tokenSource;

        public UserListPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                _tokenSource?.Cancel();

                progress.IsActive = true;
                viewSource.Source = null;

                if (e.Parameter is DiscordChannel channel)
                {
                    _channel = channel;

                    if (channel.Guild?.Members.Count != channel.Guild?.MemberCount)
                    {
                        try
                        {
                            _tokenSource = new CancellationTokenSource();
                            var task = channel.Guild.GetAllMembersAsync(_tokenSource.Token);
                            await task.ConfigureAwait(false);
                        }
                        catch
                        {
                            return;
                        }
                    }

                    var discordMembers = await Task.Run(() => channel.Users.OrderBy(g => g.DisplayName).GroupBy(g => g.Roles.Where(r => r.IsHoisted).OrderBy(r => r.Position).FirstOrDefault()).OrderByDescending(g => g.Key?.Position)).ConfigureAwait(false);
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        viewSource.Source = discordMembers;
                        progress.IsActive = false;
                    });
                }
            }
            catch
            {
                // TODO: Log
            }
        }

        private void UserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.AddedItems.FirstOrDefault();
            if (item != null)
            {
                var element = (sender as ListView).ContainerFromItem(item);
                var value = new UserFlyout() { DataContext = item };
                if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Controls.Primitives.FlyoutPlacementMode"))
                {
                    value.ShowAt(element, new FlyoutShowOptions() { Placement = FlyoutPlacementMode.Right });
                }
                else
                {
                    value.ShowAt((FrameworkElement)element);
                }
            }
        }
    }
}