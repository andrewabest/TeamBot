﻿using System;
using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;
using TeamBot.Infrastructure.Messages;
using TeamBot.Infrastructure.Slack.Models;

namespace TeamBot.Features
{
    public class BotModule : NancyModule
    {
        public BotModule(
            IMessageProcessor messageProcessor)
            : base("/bot")
        {
            if (messageProcessor == null) 
                throw new ArgumentNullException("messageProcessor");

            Post["/", true] = async (ctx, ct) =>
            {
                IncomingMessage incomingMessage = Request.Headers.ContentType == "application/json"
                    ? this.Bind<IncomingMessage>()
                    : ExtractMessage(Request.Form);

                string company = Request.Query.Company;
                string token = Request.Query.Token;

                await Task.Run(async () =>
                {
                    await messageProcessor.Process(company, token, incomingMessage);
                });

                return new Response().WithStatusCode(HttpStatusCode.OK);
            };
        }

        private IncomingMessage ExtractMessage(dynamic source)
        {
            var message = new IncomingMessage
            {
                ChannelId = source.channel_id,
                ChannelName = source.channel_name,
                TeamId = source.team_id,
                Text = source.text,
                Token = source.token,
                UserId = source.user_id,
                UserName = source.user_name,
                Command = source.command
            };

            return message;
        }
    }
}