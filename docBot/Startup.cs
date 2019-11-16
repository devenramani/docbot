// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.6.2

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using docBot.Bots;
using System;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Options;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using docBot.Accessors;

namespace docBot
{
    public class Startup
    {
        private ILoggerFactory _loggerFactory;

        private bool _isProduction = false;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, EchoBot>();

            services.AddSingleton<IConfiguration>(Configuration);

            services.AddBot<ImageProcessingBot>(options =>
            {
                var secretKey = Configuration.GetSection("botFileSecret")?.Value;
                var botFilePath = Configuration.GetSection("botFilePath")?.Value;

                // Get the Boty Config file and add it as a singleton
                var botConfig = BotConfiguration.Load(botFilePath ?? @".\ImageProcessingBot.bot", secretKey);

                services.AddSingleton(singleton => botConfig ?? throw new InvalidOperationException($"The .bot config file could not be loaded. ({botConfig})"));

                //Set up Bot End point 

                var environment = _isProduction ? "production" : "development";
                var service = botConfig.Services.Where(x => x.Type == ServiceTypes.Endpoint && x.Name == environment).FirstOrDefault();

                if (!(service is EndpointService endpointService))
                {
                    throw new InvalidOperationException($"The .bot file does not contain an endpoint with name '{environment}'.");
                }
                options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);

                //Create a Logger

                ILogger logger = _loggerFactory.CreateLogger<ImageProcessingBot>();

                options.OnTurnError = async (context, exception) =>
                {
                    logger.LogError($"Exception caught : {exception}");

                    await context.SendActivityAsync("broken bot");
                };

                IStorage storage = new MemoryStorage();

                var conversationState = new ConversationState(storage);
                options.State.Add(conversationState);

                var userState = new UserState(storage);
                options.State.Add(userState);

            });

            services.AddSingleton<ImageProcessingBotAccessors>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                if (options == null)
                {
                    throw new InvalidOperationException("BotFrameworkOptions must be configured prior to setting up the state accessors");
                }

                var conversationState = options.State.OfType<ConversationState>().FirstOrDefault();
                if (conversationState == null)
                {
                    throw new InvalidOperationException("ConversationState must be defined and added before adding conversation-scoped state accessors.");
                }

                var userState = options.State.OfType<UserState>().FirstOrDefault();

                if (userState == null)
                {
                    throw new InvalidOperationException("User State mjust be defined and added befor the conversation scoping");
                }

                // Create the custom state accessor.
                // State accessors enable other components to read and write individual properties of state.
                var accessors = new ImageProcessingBotAccessors(conversationState, userState)
                {
                    ConversationDialogState = userState.CreateProperty<DialogState>(ImageProcessingBotAccessors.DialogStateName),
                    CommandState = userState.CreateProperty<string>(ImageProcessingBotAccessors.CommandStateName)


                };

                return accessors;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            _loggerFactory = loggerFactory;

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();
            //app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
