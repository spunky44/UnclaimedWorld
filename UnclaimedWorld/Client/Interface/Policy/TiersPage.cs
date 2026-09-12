using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents.SpecialEvents;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Tiers;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Policy
{
    public class TiersPage : TabPagePanel
    {
        Grid grdTiers;

        UIComponent tierHeaderContainer; // add this to the Tab Item

        const int tierHeight = 128;
        const int tierWidth = 129; // 118;

        const int tierStartX = 51;

        /// <summary>
        /// gets set when populating
        /// </summary>
        int ratingsBarWidth;

        const int tiersToShow = 4;

        int firstTier = 0;


        const int normalTooltipWidth = 200;
        const int wideTooltipWidth = 360;


        public TiersPage(TabControl parent): base(parent)
        {
            GUIManager gui = parent.guiManager;

            parent.AddTabPage(this, "TECHNOLOGY TIERS", "Set/view current tiers");
            //parent.CreateTabItem("TIERS", "Set/view current tiers");

            tierHeaderContainer = new UIComponent(gui);
            tierHeaderContainer.Width = Width; // -tierStartX;
            tierHeaderContainer.Height = 42;

            Add(tierHeaderContainer);
            // tierHeaderContainer.X = tierStartX;


            grdTiers = FullLCDPanel.AddGridWithFixedItemHeights(gui, this, tierHeaderContainer.Height, 0);
            grdTiers.ItemHeight = tierHeight;
            grdTiers.Selectability = Grid.SelectabilityOptions.None;
            grdTiers.CanGrowInHeight = true;
            grdTiers.ScrollBarEnabled = false;
            grdTiers.RowSpacing = 6;

            PopulateStartingTiers();
        }

        /// <summary>
        /// called when scrolling right/left in the tiers grid (planned feature to allow more tiers)
        /// </summary>
        private void PopulateStartingTiers()
        {
            GUIManager gui = base.guiManager;

            int xPos = tierStartX;

            /*   int noOfTiers = GameData.Instance.Tiers.Length;

               int lastTier = firstTier + 3;
               lastTier = Math.Min(lastTier, noOfTiers);
               */

            // clear content!
            grdTiers.BeginAddingEntries();

            tierHeaderContainer.Controls.Clear();
            grdTiers.Clear();

            var ratingTypes = Enum.GetValues(typeof(RatingTypes));

            Label lblHeadingCaption = new Label(guiManager);
            lblHeadingCaption.Init(Label.LabelType.LCDHeadingBlue);
            lblHeadingCaption.Text = "TECH:";
            tierHeaderContainer.Add(lblHeadingCaption);
            lblHeadingCaption.Width = 42;

            int headerXPos = tierStartX; // 0;
            // TierType tier;
            int tierGap = 11;
            IterateVisibleTiers(tier =>
            {
                // heading:
                Label lblHeading = new Label(guiManager);
                lblHeading.Init(Label.LabelType.LCDHeadingSteelGrey);
                lblHeading.Text = (tier.Index + 1) + " - " + tier.Name.ToUpper(Config.Culture); // "NAME";
                tierHeaderContainer.Add(lblHeading);
                lblHeading.Y = 0;
                lblHeading.X = headerXPos;
                lblHeading.Width = tierWidth - tierGap;
                lblHeading.ToolTip = Common.ComposeHeadingAndBlobText(tier.Name, tier.Description);

                Label lblRating = new Label(guiManager);
                lblRating.Init(Label.LabelType.LCDNormal);
                lblRating.Text = Common.PercentageToString(tier.UpperEdge);
                tierHeaderContainer.Add(lblRating);
                lblRating.Y = lblHeading.Bottom;
                int ratingXPos = lblHeading.Right + tierGap / 2 - lblRating.Width / 2;
                ratingXPos = Common.ClampTop(ratingXPos, tierHeaderContainer.Width - lblRating.Width);
                lblRating.X = ratingXPos;

                headerXPos += tierWidth;
            });


            // vertical: data rows:
            foreach (var rating in ratingTypes)
            {
                RatingTypes ratingType = (RatingTypes)rating;

                //panel
                Color? color = null;

                switch (ratingType)
                {
                    case RatingTypes.Comfort:
                        color = Common.ColorFromHex(GameData.Instance.GUIConstants.ComfortColor);
                        break;

                    case RatingTypes.Security:
                        color = Common.ColorFromHex(GameData.Instance.GUIConstants.SecurityColor);
                        break;

                    case RatingTypes.Food:
                        color = Common.ColorFromHex(GameData.Instance.GUIConstants.FoodColor);
                        break;
                }

                LCDInnerPanel panel = new LCDInnerPanel(gui, grdTiers.Width, false, alpha: 0.5f, color: color);
                panel.Panel.Height = tierHeight;
                grdTiers.AddEntry(rating, panel.Panel);

                int buttonsLeft = 45;

                // ratings bar:
                FillableBar bar = new FillableBar(gui, FillableBar.FillableBarType.ProgressBar, false);
                //bar.Width = ;
                panel.AddContent(bar, buttonsLeft, 6);
                bar.Height = 12;
                bar.ID = UIComponent.DataControlID.Rating;
                bar.Color = color.Value;
                bar.MaxValue = 100;
                bar.DebugTag = "ratingsBar";
                bar.ShowMaxValueLabelAtEnd = false;

                // row header at the left edge:
                Icon rowHeader = new Icon(gui);
                panel.AddContent(rowHeader, 16, 3);
                rowHeader.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle(GetRowHeaderSprite(ratingType)));
                rowHeader.ResizeControlToFitImage();
                rowHeader.ToolTip = Common.ComposeHeadingAndBlobText(Statistic.RatingsTypeToString(ratingType), Statistic.RatingsTypeToDescription(ratingType));

                // buttons:
                HorizontalList hzButtons = new HorizontalList(gui);
                hzButtons.Height = tierHeight;
                hzButtons.MinHeight = tierHeight; // don't scale the height by its contents. 
                hzButtons.MaxHeight = tierHeight;
                hzButtons.ID = UIComponent.DataControlID.Actions;
                panel.AddContent(hzButtons, buttonsLeft - 4, 21);
                //tierRow.Add(hzButtons);

                hzButtons.BeginAddingEntries();

                IterateVisibleTiers(t => AddTierButton(hzButtons, ratingType, t));


                hzButtons.EndAddingEntries();

                ratingsBarWidth = hzButtons.Width + 14;
                bar.Width = ratingsBarWidth;
            }


            grdTiers.EndAddingEntries();


            Height = grdTiers.Bottom;

        }



        private void IterateVisibleTiers(Action<TierType> iterator)
        {
            int noOfTiers = GameData.Instance.Tiers.Length;

            int lastTier = firstTier + tiersToShow - 1; // 3;
            lastTier = Math.Min(lastTier, noOfTiers);

            for (int i = firstTier; i <= lastTier; i++) // horizontal
            {
                TierType tier = GameData.Instance.Tiers[i];

                iterator(tier);
            }
        }

        class TierButtonArgs : EventArgs
        {
            public TierType Tier;
            public RatingTypes Rating;
        }

        /// <summary>
        /// let's store the blobs here...
        /// </summary>
        Dictionary<TierArea, string> tierButtonTooltips = new Dictionary<TierArea, string>();

        private void AddTierButton(HorizontalList buttonList, RatingTypes rating, TierType tier)
        {
            GUIManager gui = buttonList.guiManager;
            UIComponent item = new UIComponent(gui);
            buttonList.AddEntry(tier, item);

            ImageButton btAction = new ImageButton(gui);
            item.Add(btAction);
            btAction.Init(GetActionButtonType(rating));
            btAction.Tag1 = tier; // key;
            btAction.Click += btAction_Click;
            btAction.EventArgs = new TierButtonArgs() { Tier = tier, Rating = rating };
            btAction.ID = UIComponent.DataControlID.Action;
            // btAction.Enabled = enabled;
            btAction.ScaleImageToSizeOfControl = false;
            btAction.CheckedMode = CheckedModes.CannotBeChecked;
            btAction.TooltipExpires = false;
            btAction.TooltipWidth = 240;
            TierArea tierArea = GetTierArea(rating, tier);

            string tooltip = Common.ComposeHeadingAndBlobText(tierArea.ToString(), tierArea.Description);
            tierButtonTooltips.Add(tierArea, tooltip);
            btAction.ToolTip = tooltip;


            // btAction.ToolTip = tooltip;
            // btAction.DebugTag = action.ToString() + "Button";

            item.Width = btAction.Width;
            item.Height = btAction.Height;

            int votesY = 20;
            Label lblFor = new Label(buttonList.guiManager);
            lblFor.Init(Label.LabelType.LCDNormal);
            lblFor.ID = UIComponent.DataControlID.VotersFor;
            item.Add(lblFor);
            lblFor.X = 12;
            lblFor.Y = votesY;

            Icon icon = new Icon(gui);
            icon.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle("lcd_icon_thumbsUp"), UIComponent.LCDNormal, UIComponent.LCDNormal);
            icon.ResizeControlToFitImage();
            icon.ID = UIComponent.DataControlID.VotersForIcon;
            item.Add(icon);
            icon.Y = votesY;
            icon.X = 36;
            icon.CanHaveFocus = false;

            Label lblAgainst = new Label(buttonList.guiManager);
            lblAgainst.Init(Label.LabelType.LCDNormal);
            lblAgainst.ID = UIComponent.DataControlID.VotersAgainst;
            item.Add(lblAgainst);
            lblAgainst.X = 53;
            lblAgainst.Y = votesY;

            icon = new Icon(gui);
            icon.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle("lcd_icon_thumbsDown"), UIComponent.errorColor, UIComponent.errorColor);
            icon.ResizeControlToFitImage();
            icon.ID = UIComponent.DataControlID.VotersAgainstIcon;
            item.Add(icon);
            icon.Y = votesY;
            icon.X = 85;
            icon.CanHaveFocus = false;

            Label lblPrompt = new Label(buttonList.guiManager);
            lblPrompt.Init(Label.LabelType.LCDNormal);
            lblPrompt.ID = UIComponent.DataControlID.Prompt;
            item.Add(lblPrompt);
            lblPrompt.X = 53;
            lblPrompt.Y = lblAgainst.Bottom;
            lblPrompt.TooltipWidth = normalTooltipWidth;


        }

       
        private static TierArea GetTierArea(RatingTypes rating, TierType tier)
        {
            TierArea tierArea = GameData.Instance.AllTierAreas.FirstOrDefault(a => a.Value.TierType == tier && a.Value.Area == rating).Value;
            return tierArea;
        }

        private bool CanAdopt(List<Entity> membersFor, List<Entity> membersAgainst)
        {
            return membersFor.Count > membersAgainst.Count;
        }

        void btAction_Click(UIComponent sender, EventArgs e)
        {
            TierButtonArgs args = e as TierButtonArgs;
            Expedition expedition = LookUp<Expedition, ExpeditionID>.FindByID(The.InGameUI.UIExpedition);

            TierArea tierArea = GetTierArea(args.Rating, args.Tier);

            List<Entity> membersFor, membersAgainst, unqualified; //, membersWhoWouldHavePrinciplesRaised;
            TierType previousTier;
            The.InGameUI.UIAllegiance.GetMembersInTierRange(args.Rating, args.Tier, GameData.Instance.AIConstants.AgentCanVote, out membersFor, out membersAgainst, out unqualified, /*out membersWhoWouldHavePrinciplesRaised,*/ out previousTier);

            if (CanAdopt(membersFor, membersAgainst))
            {

                AdoptTierPolicy adopt = new AdoptTierPolicy(The.InGameUI.UIExpedition.Value, args.Tier, args.Rating, true);

                sender.DebugTag = "adopted";

                The.Client.Controller.StoreAndExecuteCommand(adopt); //.Execute(true);

                PopulateTiers();

                PolicyAdoptedByVoting policyAdopted = new PolicyAdoptedByVoting();
                policyAdopted.ShowPolicyAdoption(tierArea, membersFor, membersAgainst, unqualified);
            }
        }


        private string GetRowHeaderSprite(RatingTypes rating)
        {
            switch (rating)
            {
                case RatingTypes.Comfort:
                    return "policy_banner_comfort";

                case RatingTypes.Security:
                    return "policy_banner_security";

                case RatingTypes.Food:
                    return "policy_banner_food";

            }

            return "policy_banner_comfort";

        }


        private ImageButtonType GetActionButtonType(RatingTypes rating)
        {
            switch (rating)
            {
                case RatingTypes.Comfort:
                    return ImageButtonType.ComfortPolicy;

                case RatingTypes.Security:
                    return ImageButtonType.SecurityPolicy;

                case RatingTypes.Food:
                    return ImageButtonType.FoodPolicy;

            }

            return ImageButtonType.ComfortPolicy;


        }

        Color comfortColor = Util.ColorFromHex("ECBCCF").Value;
        Color securityColor = Util.ColorFromHex("A3F1FA").Value;
        Color foodColor = Util.ColorFromHex("C0FDDD").Value;
        Color readyToAdoptColor = Common.ColorFromHex("F5FEBD");
        Color lockedColor = Common.ColorFromHex("BCE5EC");

        private Color GetButtonColor(RatingTypes rating)
        {
            switch (rating)
            {
                case RatingTypes.Comfort:
                    return comfortColor;

                case RatingTypes.Security:
                    return securityColor;

                case RatingTypes.Food:
                    return foodColor;

                default:
                    return comfortColor;

            }

        }


        private void UpdateTierButton(Expedition expedition, HorizontalList hzButtons, TierType tier, RatingTypes ratingType, float rating)
        {
            UIComponent tierComponent;
            hzButtons.TryGetEntry(tier, out tierComponent);
            ImageButton btTier = (ImageButton)tierComponent.FindChildById(UIComponent.DataControlID.Action, true);

            Color color, disabledColor;

            float requiredRating;
            if (expedition.Policy.TierIsUnlocked(ratingType, tier))
            {
                UpdateUnlockedTierArea(ratingType, tier, tierComponent, btTier, out color, out disabledColor);
            }
            else if (expedition.Policy.IsLaterTier(ratingType, tier))
            {
                UpdateLaterTierArea(ratingType, tier, tierComponent, btTier, out color, out disabledColor);
            }
            else if (!expedition.Policy.RatingIsInsideOrAbovePreviousTier(ratingType, rating, tier, out requiredRating))
            {
                UpdateTierAreaAboveRating(ratingType, tier, requiredRating, tierComponent, btTier, out color, out disabledColor);
            }
            else //( IsUnlockableTier(ratingType, rating, tier))
            {
                UpdateVotes(ratingType, tier, rating, tierComponent, out color);
                disabledColor = color; // not used               
            }


            btTier.SetSkinLocation(SkinState.Normal, null, color, color);
            btTier.SetSkinLocation(SkinState.Disabled, null, disabledColor, disabledColor);


        }

        private void UpdateTierAreaAboveRating(RatingTypes rating, TierType tier, float requiredRating, UIComponent tierComponent, ImageButton btTier, out Color color, out Color disabledColor)
        {
            TierArea tierArea = GetTierArea(rating, tier);

            HideVotes(tierComponent);
            btTier.Enabled = false;

            color = lockedColor;
            disabledColor = lockedColor;

            StringBuilder text = new StringBuilder();
            Common.Append(text, tierButtonTooltips[tierArea]);
            Common.AppendDividerOnOwnLine(text);
            Common.AppendImpossibleActionText(text, "Cannot adopt: ");
            Common.Append(text, "The colony rating has to reach ");
            Common.AppendFormat(text, "{0}", true, Common.PercentageToString(requiredRating));
            string tooltip = text.ToString();
            btTier.ToolTip = tooltip;

            Label lblPrompt = (Label)tierComponent.FindChildById(UIComponent.DataControlID.Prompt, true);
            lblPrompt.Visible = true;
            lblPrompt.ToolTip = tooltip;
            lblPrompt.Text = "CANNOT VOTE";
            lblPrompt.FitToText();
            tierComponent.CenterChildHorizontally(lblPrompt);

        }

        private void UpdateLaterTierArea(RatingTypes rating, TierType tier, UIComponent tierComponent, ImageButton btTier, out Color color, out Color disabledColor)
        {
            TierArea tierArea = GetTierArea(rating, tier);

            HideVotes(tierComponent);
            btTier.Enabled = false;

            color = lockedColor;
            disabledColor = lockedColor;

            StringBuilder text = new StringBuilder();
            Common.Append(text, tierButtonTooltips[tierArea]);
            Common.AppendDividerOnOwnLine(text);
            Common.AppendImpossibleActionText(text, "Cannot adopt: ");
            Common.Append(text, "We need to adopt the lower tier areas first.");
            btTier.ToolTip = text.ToString();
        }

        private void UpdateUnlockedTierArea(RatingTypes ratingType, TierType tier, UIComponent tierComponent, ImageButton btTier, out Color color, out Color disabledColor)
        {
            TierArea tierArea = GetTierArea(ratingType, tier);

            HideVotes(tierComponent);
            color = GetButtonColor(ratingType);
            btTier.Enabled = false;
            disabledColor = color;

            StringBuilder text = new StringBuilder();
            Common.Append(text, tierButtonTooltips[tierArea]);
            Common.AppendDividerOnOwnLine(text);
            Common.AppendImpossibleActionText(text, "Cannot adopt: ");
            Common.Append(text, "We have already adopted this policy.");
            btTier.ToolTip = text.ToString();
        }

        private int MapRatingToFillableBar(float rating)
        {
            int tierIndex;
            TierType tier = Common.GetStairStepIndex(rating, GameData.Instance.Tiers, out tierIndex);
            TierType previousTier;
            float lowerTierEdge;

            // tiers should be pageable...
            if (tier.Index < firstTier)
            {
                // below visible range
                return 0;
            }
            else if (tier.Index >= firstTier + tiersToShow)
            {
                // above visible range
                return 100;
            }
            else
            {
                TierType.GetTierBelow(tierIndex, out previousTier, out lowerTierEdge);

                tierIndex -= firstTier; // make it start at 0

                // 0 - 1
                float tierProgress = (rating - lowerTierEdge) / (tier.UpperEdge - lowerTierEdge);


                float scaledRating = MathHelper.Lerp(tierIndex * tierWidth, (tierIndex + 1) * tierWidth, tierProgress);
                scaledRating /= ratingsBarWidth;

                int value = (int)Math.Round(100f * scaledRating);

                return Common.Clamp(value, 0, 100);
            }
        }

        /// <summary>
        /// called when refreshing tier data
        /// </summary>
        private void PopulateTiers()
        {
            Expedition expedition = LookUp<Expedition, ExpeditionID>.FindByID(The.InGameUI.UIExpedition);
            Allegiance allegiance = The.InGameUI.UIAllegiance;

            if (expedition == null)
                return;

            var ratingTypes = Enum.GetValues(typeof(RatingTypes));

            foreach (var item in ratingTypes)
            {
                UIComponent row;
                RatingTypes ratingType = (RatingTypes)item;

                if (grdTiers.TryGetEntry(item, out row))
                {
                    FillableBar ratingsBar = (FillableBar)row.FindChildById(UIComponent.DataControlID.Rating, true);

                    float rating = allegiance.Statistics.GetRating(ratingType);
                    ratingsBar.Value = MapRatingToFillableBar(rating);
                    ratingsBar.ToolTip = Common.ComposeHeadingAndBlobText("Colony " + Statistic.RatingsTypeToString(ratingType).ToLower(Config.Culture) + " conditions", Common.PercentageToString(rating));

                    HorizontalList hzButtons = (HorizontalList)row.FindChildById(UIComponent.DataControlID.Actions, true);

                    IterateVisibleTiers(t =>
                            UpdateTierButton(expedition, hzButtons, t, ratingType, rating)
                        );

                }
            }


            /*
            foreach (var item in expedition.Policy.CurrentTiers)
            {
                UIComponent row;
                if (grdTiers.TryGetEntry(item.Key, out row))
                {
                    FillableBar ratingsBar = (FillableBar)row.FindChildById(UIComponent.DataControlID.Rating, true);
                    ratingsBar.Value = (int)Math.Round(100f * allegiance.Statistics.GetRating(item.Key));

                    HorizontalList hzButtons = (HorizontalList)row.FindChildById(UIComponent.DataControlID.Actions, true);

                    ImageButton btTier;
                    UIComponent tierComponent;
                    hzButtons.TryGetEntry(item.Value, out tierComponent);
                   // btTier = tierComponent as ImageButton;
                    UpdateVotes(item.Key, item.Value, tierComponent);

                }
            }  */

        }


        private void HideVotes(UIComponent item)
        {
            Label lblPro = (Label)item.FindChildById(UIComponent.DataControlID.VotersFor, true);
            lblPro.Visible = false;

            Icon icPro = (Icon)item.FindChildById(UIComponent.DataControlID.VotersForIcon, true);
            icPro.Visible = false;

            Label lblAgainst = (Label)item.FindChildById(UIComponent.DataControlID.VotersAgainst, true);
            lblAgainst.Visible = false;

            Icon icAgainst = (Icon)item.FindChildById(UIComponent.DataControlID.VotersAgainstIcon, true);
            icAgainst.Visible = false;


            Label lblPrompt = (Label)item.FindChildById(UIComponent.DataControlID.Prompt, true);
            lblPrompt.Visible = false;
        }

        private List<Entity> GetUnavailableVoters(List<Entity> voters) // List<Entity> membersAboveRange, List<Entity> membersBelowRange)
        {
            List<Entity> votersNotAvailable = null;
            foreach (var item in voters)
            {
                if (!GameData.Instance.AIConstants.PolicyCanBeAdopted.IsFulfilled(item))
                {
                    Common.AddToList(ref votersNotAvailable, item);
                }
            }

            return votersNotAvailable;

        }

        private void UpdateVotes(RatingTypes rating, TierType tier, float currentRating, UIComponent item, out Color color) // ImageButton button)
        {
            TierArea tierArea = GetTierArea(rating, tier);

            // get the number of members with principles in this range:
            int membersAboveLimit;
            int membersBelowLimit;
            List<Entity> membersAboveRange, membersBelowRange, unqualified;
            TierType previousTier;
            The.InGameUI.UIAllegiance.GetMembersInTierRange(rating, tier, GameData.Instance.AIConstants.AgentCanVote, out membersAboveRange, out membersBelowRange, out unqualified, out previousTier);

            if (membersAboveRange != null)
            {
                membersAboveLimit = membersAboveRange.Count;
            }
            else
            {
                membersAboveLimit = 0;
            }

            if (membersBelowRange != null)
            {
                membersBelowLimit = membersBelowRange.Count;
            }
            else
            {
                membersBelowLimit = 0;
            }


            List<Entity> voters = null;
            Common.AddRangeToList(ref voters, membersAboveRange);
            Common.AddRangeToList(ref voters, membersBelowRange);

            List<Entity> unavailable = GetUnavailableVoters(voters);

            Icon icPro = (Icon)item.FindChildById(UIComponent.DataControlID.VotersForIcon, true);
            icPro.Visible = true;

            Icon icAgainst = (Icon)item.FindChildById(UIComponent.DataControlID.VotersAgainstIcon, true);
            icAgainst.Visible = true;

            Label lblPro = (Label)item.FindChildById(UIComponent.DataControlID.VotersFor, true);
            lblPro.Text = membersAboveLimit.ToString() + "X";
            lblPro.AlignRight(32);
            lblPro.Visible = true;

            if (membersAboveRange != null)
            {
                lblPro.ToolTip = "Members for: \n" + Common.ListToCommaSeparatedString(membersAboveRange, e => e.GetDisplayName());
            }
            else
            {
                lblPro.ToolTip = "No one is for adopting this.";
            }


            Label lblAgainst = (Label)item.FindChildById(UIComponent.DataControlID.VotersAgainst, true);
            lblAgainst.Text = membersBelowLimit.ToString() + "X";
            lblAgainst.AlignRight(82);
            lblAgainst.Visible = true;

            if (membersBelowRange != null)
            {
                lblAgainst.ToolTip = "Members against: \n" + Common.ListToCommaSeparatedString(membersBelowRange, e => e.GetDisplayName());
            }
            else
            {
                lblAgainst.ToolTip = "No one is against adopting this.";
            }

            ImageButton btTier = (ImageButton)item.FindChildById(UIComponent.DataControlID.Action);
            Label lblPrompt = (Label)item.FindChildById(UIComponent.DataControlID.Prompt, true);
            lblPrompt.Visible = true;


            if (unavailable == null || unavailable.Count == 0)
            {
                if (membersAboveLimit > membersBelowLimit)
                {
                    lblPrompt.Text = "DECIDE NOW?";
                    lblPrompt.ToolTip = string.Format("Click to adopt this policy. We will then be able to buy and produce items from this tier. \n \nNOTE: When adopting this policy, those who are against will have their principles raised to this level ({0}). This can increase their unhappiness unless conditions are quickly improved.",
                        Common.PercentageToString(GameData.Instance.Tiers[tier.Index - 1].UpperEdge));
                    lblPrompt.TooltipWidth = normalTooltipWidth;
                    lblPrompt.TooltipExpires = true;

                    StringBuilder text = new StringBuilder();
                    Common.Append(text, tierButtonTooltips[tierArea]);
                    Common.AppendDividerOnOwnLine(text);
                    Common.AppendPossibleActionText(text, "CLICK TO ADOPT THIS POLICY");
                    btTier.ToolTip = text.ToString();

                    btTier.Enabled = true;
                    color = readyToAdoptColor;

                }
                else
                {
                    int noOfVoters = membersBelowLimit + membersAboveLimit;

                    int majority = (int)Math.Ceiling(noOfVoters / 2f); // noOfVoters / 2;
                    if (noOfVoters % 2 == 0) 
                    {
                        majority++;
                    }

                    int neededExtraVotes = majority - membersAboveLimit;

                    lblPrompt.Text = "NO MAJORITY";
                    lblPrompt.ToolTip = ComposeNoMajorityTooltip(tier, rating, currentRating, neededExtraVotes, membersBelowRange);
                    lblPrompt.TooltipWidth = wideTooltipWidth;
                    lblPrompt.TooltipExpires = false;

                    StringBuilder text = new StringBuilder();
                    Common.Append(text, tierButtonTooltips[tierArea]);
                    Common.AppendDividerOnOwnLine(text);
                    Common.AppendImpossibleActionText(text, "Cannot adopt: ");
                    Common.Append(text, "No majority. Learn more by hovering on the NO MAJORITY text.");
                    btTier.ToolTip = text.ToString();

                    btTier.Enabled = false;
                    color = lockedColor;
                }
            }
            else
            {
                lblPrompt.Text = "NOT READY";
                lblPrompt.ToolTip = "The following members are not ready to vote right now: \n" + Common.ListToCommaSeparatedString(unavailable, e => e.GetDisplayName());
                lblPrompt.TooltipWidth = normalTooltipWidth;
                lblPrompt.TooltipExpires = true;

                StringBuilder text = new StringBuilder();
                Common.Append(text, tierButtonTooltips[tierArea]);
                Common.AppendDividerOnOwnLine(text);
                Common.AppendImpossibleActionText(text, "Cannot adopt: ");
                Common.Append(text, "Some members are busy or sleeping. Everyone must be able to participate in the vote.");
                btTier.ToolTip = text.ToString();

                btTier.Enabled = false;
                color = lockedColor;
            }

            lblPrompt.FitToText();
            item.CenterChildHorizontally(lblPrompt);

        }


        private string ComposeNoMajorityTooltip(TierType tier, RatingTypes rating, float currentRating, int neededVotes, List<Entity> membersBelowRange)
        {
           
            /*There is no majority for this policy yet. \n \n
             To adopt this policy, more colonists with higher principles are needed: Either from immigration or from current colonists gradually increasing their principles:

                SUGGESTED STRATEGY:
                Do one or both of the following:
                1. Let the inhabitants grow accustomed to the conditions and they will slowly change their principles to match the COMFORT rating.
                Current time to reach majority: 12d / not possible with current rating
	                After increasing COMFORT rating to 32%, the estimated time to reach majority is: 6d (on 180-8-2)
                2. Attract immigrants with high COMFORT principles (above 32%) to more quickly gain a majority.
                         
             */

           // float timeToReachMajority =
            DateAndTime.TimeDateYear timeToReachMajority = The.InGameUI.UIAllegiance.GetTimeToReachMajority(rating, tier, /*currentRating,*/ neededVotes, membersBelowRange);

           // timeToReachMajority.ConvertToAbsoluteTime();

            StringBuilder text = new StringBuilder();
            Common.AppendHeaderOnLightBG(text, "No majority");
            Common.Append(text, "There is no majority for this policy yet.");
            Common.AppendLine(text);
            Common.AppendLine(text);
            Common.Append(text, "To adopt this policy, more colonists with higher principles are needed"); //: Either from immigration or from current colonists gradually increasing their principles.");
            Common.AppendLine(text);
            Common.AppendLine(text);
            Common.AppendHeaderOnLightBG(text, "SUGGESTED STRATEGY:");
            Common.AppendLine(text, "Do one or both of the following:");
            Common.AppendLine(text);
            Common.AppendLine(text, "1. Let the inhabitants grow accustomed to a higher " + Statistic.AppendRatingsTypeToStringAndIcon(rating) + " rating.");
            Common.AppendIndentedLine(text, "They will slowly change their principles above the rating.");
          //  Common.AppendIndentedLine(text, "Current time to reach majority: " + timeToReachMajority);
            float tierEdgeBelow = tier.GetTierEdgeBelow();
            string timeString = timeToReachMajority.ToIntervalString();
            if (currentRating < tierEdgeBelow)
            {
               // float newTimeToReachMajority = The.InGameUI.UIAllegiance.GetTimeToReachMajority(rating, tier, tier.Edge, neededVotes, membersBelowRange);

                Common.AppendIndentedLine(text, "After increasing " + Statistic.AppendRatingsTypeToStringAndIcon(rating) +
                    " to " + Common.PercentageToString(tierEdgeBelow, valueTint: Common.ValueTint.Neutral)
                     + " the estimated time to reach majority is: " + timeString);
            }
            else
            {
                Common.AppendIndentedLine(text, "Current time to reach majority: " + timeString);
            }

            Common.AppendLine(text, "2. Attract immigrants with high " + Statistic.AppendRatingsTypeToStringAndIcon(rating) + " principles.");
            Common.AppendIndentedLine(text, "This will more quickly gain a majority.");

            return text.ToString(); //"There is no majority for this policy yet. \n \nTo adopt this policy, more colonists with higher principles are needed: Either from immigration or from current colonists gradually increasing their principles.";


        }

      

        public override void Refresh()
        {
            Populate();
        }

        private void Populate()
        {
            PopulateTiers();

        }
    }
}
