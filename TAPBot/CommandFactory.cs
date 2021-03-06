﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using SteamKit2;

namespace TAPBot
{

    // A factory class to return the appropriate BotAction object based on the parsed chat command, if any

    class CommandFactory
    {
        private List<BotAction> actions;
        private SteamFriends steamFriends;

        public CommandFactory(SteamFriends steamFriends)
        {
            this.steamFriends = steamFriends;
            actions = new List<BotAction>();
            Initialize();
        }

        protected CommandFactory()
        {
            actions = new List<BotAction>();
            Initialize();
        }

        public void Initialize()
        {
            DealPicker dealPicker = new DealPicker();

            //------------------------------------------------------------------------

            // finds the balance (co-op shop points) of the invoker
            // Specific to The After Party steam group

            actions.Add(new BalanceBotAction());

            //------------------------------------------------------------------------

            // determines which member has the most co-op shop points.
            // Specific to The After Party steam group

            actions.Add(new QueenBotAction());

            //------------------------------------------------------------------------

            // rolls a number between the specified values, or if no values are specified it rolls a random number between 1-100

            actions.Add(new RollAction());

            //------------------------------------------------------------------------

            // a trivial action to count how many times !milkshakes has been invoked
            // no real purpose

            actions.Add(new MilkshakeAction());

            //------------------------------------------------------------------------

            // creates a daily deal for this day
            // Specific to The After Party steam group

            DealWrapper deal = new DealWrapper(dealPicker.PickDeal(new Random(Convert.ToInt32((DateTime.Today - new DateTime(2010, 1, 1)).TotalDays)), 1));
            DailyDealAction dealAction = new DailyDealAction(dealPicker, deal);
            actions.Add(dealAction);

            //------------------------------------------------------------------------

            // if the current deal is bought, there is need to reroll the deal to get a new random deal
            // otherwise it uses the same seed (a value procurred via today's date times a reroll factor)
            // and will just pick the next item on the list after the purchased item is removed
            // Specific to The After Party steam group

            actions.Add(new RerollAction(dealAction));

            //------------------------------------------------------------------------

            // resets the reroll counter in the dailyDealAction 
            // only successfully executes if Monukai invokes it personally
            // Specific to The After Party steam group

            actions.Add(new ResetAction(dealAction));

            //------------------------------------------------------------------------

            // The buy deal action gives users a chance to reserve the current deal
            // It is complemented by a confirm action that confirms their purchase

            LinkedList<UserEntry> users = new LinkedList<UserEntry>();
            DateTimeWrapper date = new DateTimeWrapper();

            actions.Add(new BuyDealAction(users, date, dealAction, deal));
            actions.Add(new ConfirmBuyAction(users, date, dealAction, deal));

            //------------------------------------------------------------------------

            // The spin action "spins" a number of deals equal to the number specified in the command
            // -- uses current datetime to seed randomness
            // Specific to The After Party steam group

            actions.Add(new SpinAction(dealPicker));

            //------------------------------------------------------------------------

            // parses steamApp links to see if the specified game/app is in the co-op shop
            // Specific to The After Party steam group

            actions.Add(new ParseAppIDAction());

            //------------------------------------------------------------------------

            // simple action to join a specified chatroom by it's steamcommunity URL

            actions.Add(new JoinAction());

            //------------------------------------------------------------------------

            // report the player of the week of the TAP group

            actions.Add(new POTWAction());

            //------------------------------------------------------------------------

            // simple action to post directory URLs to help easily navigate users to certain pages

            actions.Add(new DirectoryAction());

            //------------------------------------------------------------------------

            // repost what the user said after the /me command as RP text

            actions.Add(new MeAction());

            //------------------------------------------------------------------------

            // simple action to tell the user who invokes the command their 64-bit Steam ID

            actions.Add(new IDAction());

            //------------------------------------------------------------------------

            // simple action to tell the user who invokes the command their 64-bit Steam ID

            actions.Add(new SearchAction());
        }
       
        public void ParseChatText(BotContext botContext)
        {
            foreach (BotAction action in actions)
            {
                if (action.IsValidCommand(botContext.Command.Trim().ToLower()))
                {
                    action.Execute(botContext);
                    //break;
                }
            }
        }
    }
}
