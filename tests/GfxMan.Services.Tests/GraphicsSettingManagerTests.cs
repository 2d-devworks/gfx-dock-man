using FluentAssertions;
using GfxMan.Services.Interfaces;
using GfxMan.Services.Model;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GfxMan.Services.Tests;

public class GraphicsSettingManagerTests
{
    private class WatcherCanceller
    {
        public CancellationTokenSource CancellationTokenSource { get; private set; } = new ();
        
        public void ResetTokenSource() => CancellationTokenSource = new CancellationTokenSource();
        
        public void Cancel() => CancellationTokenSource.Cancel();
    }
    
    private static void RunEventHandlerTest(GraphicsSettingManager target, WatcherCanceller watcherCanceller, IHasOnStatusChangedEvent watchedService, Action setNewStatus, string expectedActiveConfig)
    {
        Task.Run(async () => await target.WatchForChanges(watcherCanceller.CancellationTokenSource.Token), watcherCanceller.CancellationTokenSource.Token);
        
        var counter = 0;
        while (!watcherCanceller.CancellationTokenSource.IsCancellationRequested)
        {
            switch (counter)
            {
                case > 20:
                    watcherCanceller.Cancel();
                    throw new TimeoutException();
                case 4:
                    setNewStatus();
                    watchedService.OnStatusChanged += Raise.Event();
                    break;
            }

            Thread.Sleep(250);
            counter++;
        }
        
        target.ActiveConfig.Should().Be(expectedActiveConfig);
    }

    private class TestScope
    {
        public TestScope(GfxManConfiguration config)
        {
            var fileService = Substitute.For<ISettingsFileService>();
            fileService.LoadConfiguration().Returns(config);
            
            var displayAdapterService = Substitute.For<IPrimaryDisplayAdapterService>();
            displayAdapterService.PrimaryDisplayAdapter.Returns(new DisplayAdapterInfo());
            displayAdapterService.When(n => n.WatchForChanges(Arg.Any<CancellationToken>()))
                .Do(x =>
                {
                    // since we have some async code, just run a loop in the dependency that still gets cancelled by the token
                    while (!x.Arg<CancellationToken>().IsCancellationRequested)
                    {
                        Thread.Sleep(250);
                    }
                });
            
            DisplayAdapterService = displayAdapterService;
            
            var monitorService = Substitute.For<IPrimaryMonitorService>();
            monitorService.PrimaryMonitor.Returns(new MonitorInfo());
            MonitorService = monitorService;
            
            TestService = new GraphicsSettingManager(Substitute.For<ILogger<GraphicsSettingManager>>(), 
                fileService, 
                displayAdapterService, monitorService);
        }

        public IPrimaryDisplayAdapterService DisplayAdapterService { get; }
        public IPrimaryMonitorService MonitorService { get; }
        
        public GraphicsSettingManager TestService { get; }
    }
    
    [Fact]
    public void
        ActiveConfig_Should_ChangeOnDisplayAdapterChangeEvents_When_WatchingForChanges_And_ConfigForDisplayAdapterExists()
    {
        const string testName = "Test";
        var config = new GfxManConfiguration();
        config.ConfigurationOptions.Add(new GfxConfigOption()
        {
            Name = testName,
            PrimaryDisplayDriverName = testName
        });
        
        var testScope = new TestScope(config);

        var watcherCanceller = new WatcherCanceller();
        var watchedService = testScope.DisplayAdapterService;
        
        var target = testScope.TestService;
        target.OnStatusChanged += (_, _) =>
        {
            watcherCanceller.Cancel();
        };
        
        RunEventHandlerTest(target, watcherCanceller, watchedService, () =>
        {
            watchedService.PrimaryDisplayAdapter.Returns(new DisplayAdapterInfo()
            {
                Name = testName,
                IsOk = true
            });
        },testName);
        
        watcherCanceller.ResetTokenSource();
        
        RunEventHandlerTest(target, watcherCanceller, watchedService, () =>
        {
            watchedService.PrimaryDisplayAdapter.Returns(new DisplayAdapterInfo()
            {
                Name = GfxConfigOption.DefaultDisplayDriverName,
                IsOk = true
            });
        }, GfxConfigOption.DefaultConfigName);
    }
    
    [Fact]
    public void ActiveConfig_Should_ChangeOnMonitorChangeEvents_When_WatchingForChanges_And_ConfigForDisplayAdapterAndMonitorResolutionExists()
    {
        const string testName = "Test";
        var config = new GfxManConfiguration();

        var defaultMonitorInfo = new MonitorInfo()
        {
            MaximumPixelHeight = 1080,
            MaximumPixelWidth = 1920
        };

        var ultraWideMonitorInfo = new MonitorInfo()
        {
            MaximumPixelHeight = 1440,
            MaximumPixelWidth = 5120
        };
        
        config.ConfigurationOptions.Add(new GfxConfigOption()
        {
            Name = testName,
            PrimaryDisplayDriverName = GfxConfigOption.DefaultDisplayDriverName,
            PrimaryDisplayMaxHeight = ultraWideMonitorInfo.MaximumPixelHeight,
            PrimaryDisplayMaxWidth = ultraWideMonitorInfo.MaximumPixelWidth,
        });
        
        var testScope = new TestScope(config);

        var watcherCanceller = new WatcherCanceller();
        var watchedService = testScope.MonitorService;
        var target = testScope.TestService;
      
        target.OnStatusChanged += (_, _) =>
        {
            watcherCanceller.Cancel();
        };
        
        RunEventHandlerTest(target, watcherCanceller, watchedService, () =>
        {
            watchedService.PrimaryMonitor.Returns(ultraWideMonitorInfo);
        }, testName);
        
        watcherCanceller.ResetTokenSource();
        
        RunEventHandlerTest(target, watcherCanceller, watchedService, () =>
        {
            watchedService.PrimaryMonitor.Returns(defaultMonitorInfo);
        }, GfxConfigOption.DefaultConfigName);
    }

    [Fact]
    public void StatusChangedEvent_Should_BeInvoked_When_ActiveConfigurationChanges()
    {
        const string testName = "Test";
        var config = new GfxManConfiguration();
        config.ConfigurationOptions.Add(new GfxConfigOption()
        {
            Name = testName,
            PrimaryDisplayDriverName = testName
        });

        var statusChanged = false;
        
        var fileService = Substitute.For<ISettingsFileService>();
        fileService.LoadConfiguration().Returns(config);
        
        var displayAdapterService = Substitute.For<IPrimaryDisplayAdapterService>();
        displayAdapterService.PrimaryDisplayAdapter.Returns(new DisplayAdapterInfo() { Name = GfxConfigOption.DefaultDisplayDriverName, IsOk = true });
        var monitorService = Substitute.For<IPrimaryMonitorService>();
        monitorService.PrimaryMonitor.Returns(new MonitorInfo() { MaximumPixelHeight = 1080, MaximumPixelWidth = 1920 });
        var target = new GraphicsSettingManager(Substitute.For<ILogger<GraphicsSettingManager>>(), 
            fileService, 
            displayAdapterService, monitorService);

        target.OnStatusChanged += (_, _) =>
        {
            statusChanged = true;
        };
        target.ChangeActiveConfiguration(testName);
        statusChanged.Should().BeTrue();
        target.ActiveConfig.Should().Be(testName);
    }

    [Fact]
    public void ChangeActiveConfig_Should_DoNothing_When_NewActiveConfigurationIsTheSame()
    {
        const string testName = "Test";
        var config = new GfxManConfiguration();
        config.ConfigurationOptions.Add(new GfxConfigOption()
        {
            Name = testName,
            PrimaryDisplayDriverName = testName
        });

        var statusChanged = false;
        
        var testScope = new TestScope(config);
        var target = testScope.TestService;

        target.OnStatusChanged += (_, _) =>
        {
            statusChanged = true;
        };
        target.ChangeActiveConfiguration(GfxConfigOption.DefaultConfigName);
        statusChanged.Should().BeFalse();
        target.ActiveConfig.Should().Be(GfxConfigOption.DefaultConfigName);
    }

    [Fact]
    public void ChangeActiveConfig_Should_DoNothing_When_NewActiveConfigurationDoesNotExist()
    {
        const string testName = "Test";
        var config = new GfxManConfiguration();
        config.ConfigurationOptions.Add(new GfxConfigOption()
        {
            Name = testName,
            PrimaryDisplayDriverName = testName
        });

        var statusChanged = false;
        
        var testScope = new TestScope(config);
        var target = testScope.TestService;

        target.OnStatusChanged += (_, _) =>
        {
            statusChanged = true;
        };
        target.ChangeActiveConfiguration("Something");
        statusChanged.Should().BeFalse();
        target.ActiveConfig.Should().Be(GfxConfigOption.DefaultConfigName);
    }
}