using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.Messaging.Commands;
using Shield.Messaging.RawData;
using Shield.Timestamps;
using ShieldTests.Messaging.Commands.Parts;
using static Shield.Enums.Command;

namespace ShieldTests.Messaging.Commands
{
    public static class CommandsTestObjects
    {
        private static IdGenerator _idGenerator = new IdGenerator(4);

        public static CommandFactory GetProperAlwaysValidCommandFactory()
        {
            var partFactory = PartFactoryTestObjects.GetAlwaysValidPartFactory();
            var comfacadap = new CommandFactoryAutoFacAdapter((id, hostid, target, order, data, Timestamp) => new Command(id, hostid, target, order, data, TimestampFactory.Timestamp));

            return new CommandFactory(DefaultProtocolValues.Separator, partFactory, comfacadap);
            
        }

        public static ICommand GetProperTestCommand_order()
        {
            var commandFac = GetProperAlwaysValidCommandFactory();

            return commandFac.TranslateFrom(new RawCommand($"{_idGenerator.GetNewID()}*HOSTID*TARGET*ORDER*DATA"));
        }

        public static ICommand GetProperTestCommand_order(string id)
        {
            var commandFac = GetProperAlwaysValidCommandFactory();

            return commandFac.TranslateFrom(new RawCommand($"{id}*HOSTID*TARGET*ORDER*DATA"));
        }

        public static ICommand GetProperTestCommand_confirmation()
        {
            var commandFac = GetProperAlwaysValidCommandFactory();

            return commandFac.TranslateFrom(new RawCommand($"{_idGenerator.GetNewID()}*HOSTID*{Shield.GlobalConfig.DefaultTargets.ConfirmationTarget}*00ID*Valid"));
        }
        public static ICommand GetProperTestCommand_confirmation(string confirmId)
        {
            var commandFac = GetProperAlwaysValidCommandFactory();

            return commandFac.TranslateFrom(new RawCommand($"{_idGenerator.GetNewID()}*HOSTID*{Shield.GlobalConfig.DefaultTargets.ConfirmationTarget}*{confirmId}*Valid"));
        }

        public static ICommand GetProperTestCommand_reply()
        {
            var commandFac = GetProperAlwaysValidCommandFactory();

            return commandFac.TranslateFrom(new RawCommand($"{_idGenerator.GetNewID()}*HOSTID*{Shield.GlobalConfig.DefaultTargets.ReplyTarget}*00ID*ReplyData"));
        }

        public static ICommand GetProperTestCommand_reply(string replyToId)
        {
            var commandFac = GetProperAlwaysValidCommandFactory();
            ICommand d;

            return d =  commandFac.TranslateFrom(new RawCommand($"{_idGenerator.GetNewID()}*HOSTID*{Shield.GlobalConfig.DefaultTargets.ReplyTarget}*{replyToId}*ReplyData"));
        }

        public static ICommand GetInvalidCommand()
        {
            var partFactory = PartFactoryTestObjects.getAllwaysInvalidPartFactory();
            return new Command(partFactory.GetPart(PartType.ID, "INVALIDID"),
                partFactory.GetPart(PartType.HostID, "INVALIDHOSTID"),
                partFactory.GetPart(PartType.Target, "INVALIDTARGET"),
                partFactory.GetPart(PartType.Order, "INVALIDORDER"), partFactory.GetPart(PartType.Data, "INVALIDDATA"),
                TimestampFactory.Timestamp);
        }
    }
}
