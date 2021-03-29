﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Essentials
{
	public static class EssentialsExtensions
	{
		public static IAppHostBuilder ConfigureEssentials(this IAppHostBuilder builder, Action<HostBuilderContext, IEssentialsBuilder> configureDelegate)
		{
			builder.ConfigureLifecycleEvents((ctx, life) =>
			{
#if __ANDROID__
				Platform.Init(MauiApplication.Current);

				life.AddAndroid(android => android
					.OnRequestPermissionsResult((activity, requestCode, permissions, grantResults) =>
					{
						Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
					})
					.OnNewIntent((activity, intent) =>
					{
						Platform.OnNewIntent(intent);
					})
					.OnResume((activity) =>
					{
						Platform.OnResume();
					}));
#endif
			});

			builder.ConfigureServices<EssentialsBuilder>(configureDelegate);

			return builder;
		}

		public static IEssentialsBuilder AddAppAction(this IEssentialsBuilder essentials, string id, string title, string subtitle = null, string icon = null) =>
			essentials.AddAppAction(new AppAction(id, title, subtitle, icon));

		class EssentialsBuilder : IEssentialsBuilder, IServiceCollectionBuilder
		{
			string _mapServiceToken;
			Action<AppAction> _appActionHandlers;
			readonly List<AppAction> _appActions = new List<AppAction>();

			public IEssentialsBuilder UseMapServiceToken(string token)
			{
				_mapServiceToken = token;
				return this;
			}

			public IEssentialsBuilder AddAppAction(AppAction appAction)
			{
				_appActions.Add(appAction);
				return this;
			}

			public IEssentialsBuilder OnAppAction(Action<AppAction> action)
			{
				_appActionHandlers += action;
				return this;
			}

			public async void Build(IServiceCollection services)
			{
#if WINDOWS
				// Platform.MapServiceToken = _mapServiceToken;
#endif
				AppActions.OnAppAction += HandleOnAppAction;

				await AppActions.SetAsync(_appActions);
			}

			void HandleOnAppAction(object sender, AppActionEventArgs e)
			{
				_appActionHandlers?.Invoke(e.AppAction);
			}
		}
	}

	public interface IEssentialsBuilder
	{
		IEssentialsBuilder UseMapServiceToken(string token);

		IEssentialsBuilder AddAppAction(AppAction appAction);

		IEssentialsBuilder OnAppAction(Action<AppAction> action);
	}
}