﻿using System;
using System.Collections.Generic;
using System.Linq;
using Rage.Forms;
using Gwen;
using Gwen.Control;
using Rage;
using LSPD_First_Response;
using LSPD_First_Response.Engine.Scripting.Entities;
using ComputerPlus.Controllers.Models;
using ComputerPlus.Extensions.Gwen;
using ComputerPlus.Extensions.Rage;
using ComputerPlus.Controllers;
using ComputerPlus.Interfaces.Reports.Models;
using ComputerPlus.Interfaces.Reports.Arrest;
using ComputerPlus.Interfaces.Common;
using GwenSkin = Gwen.Skin;
using SystemDrawing = System.Drawing;

namespace ComputerPlus.Interfaces.ComputerPedDB
{
    public class ComputerPedView : Base
    {
        public enum QuickActions { PLACEHOLDER = 0, CREATE_ARREST_REPORT = 1, CREATE_TRAFFIC_CITATION = 2 };

        //public delegate void QuickActionSelected(object sender, QuickActions action);
        /*
        QuickActionSelected OnQuickActionSelected;
        ComputerPlusEntity mEntity;
        public ComputerPlusEntity Entity
        {
            get { return mEntity; }
            set
            {

                if (mEntity != value)
                {
                    mEntity = value;
                    if (this.IsVisible)
                        BindData();                    
                }
                else
                    mEntity = value;
            }
        }
        */

        LabeledComponent<StateControlledTextbox> text_first_name, text_last_name,
               text_home_address, text_dob, text_license_status,
               text_wanted_status_false, text_times_stopped, text_age;

        LabeledComponent<StateControlledMultilineTextbox> text_wanted_status_true;

        //LabeledComponent<Label> lbl_first_name, lbl_last_name,
//               lbl_home_address, lbl_dob, lbl_license_status,
//               lbl_wanted_status, lbl_times_stopped, lbl_age; // lbl_alert

        ImagePanel ped_image_holder;

        ComboBox cb_action;

        private DetailedEntity DetailedEntity;
        private Ped ThePed
        {
            get
            {
                return DetailedEntity.Entity.Ped;
            }
        }

        internal static int DefaultHeight = 630;
        internal static int DefaultWidth = 730;

        FormSection pedInformation;
        Base pedContent;

        SystemDrawing.Color labelColor = SystemDrawing.Color.Black;
        Font labelFont;

        bool BindNeeded;

        internal event QuickActionSelected OnQuickActionSelected;
        internal delegate void QuickActionSelected(object sender, QuickActions action);

        internal ComputerPedView(Base parent, DetailedEntity pedReport, QuickActionSelected quickActionCallback = null) : base(parent)
        {
            DetailedEntity = pedReport;
            InitializeLayout();
            BindNeeded = true;
            if (quickActionCallback != null) OnQuickActionSelected += quickActionCallback;
            else OnQuickActionSelected += OnQuickAction;
        }

        private void InitializeLayout()
        {
            Function.LogDebug("InitializeLayout ComputerPedView");


            labelFont = this.Skin.DefaultFont.Copy();
            labelFont.Size = 14;
            labelFont.Smooth = true;

            cb_action = new ComboBox(this);

            pedInformation = new FormSection(this, "Person Information");
            pedContent = new Base(this);

            text_first_name = LabeledComponent.StatefulTextbox(pedContent, "First Name", RelationalPosition.TOP, Configs.BaseFormControlSpacingHalf, labelColor, labelFont);
            text_last_name = LabeledComponent.StatefulTextbox(pedContent, "Last Name", RelationalPosition.TOP, Configs.BaseFormControlSpacingHalf, labelColor, labelFont);
            text_age = LabeledComponent.StatefulTextbox(pedContent, "Age", RelationalPosition.TOP, Configs.BaseFormControlSpacingHalf, labelColor, labelFont);

            text_home_address = LabeledComponent.StatefulTextbox(pedContent, "Home Address", RelationalPosition.TOP, Configs.BaseFormControlSpacingHalf, labelColor, labelFont);
            text_dob = LabeledComponent.StatefulTextbox(pedContent, "DOB", RelationalPosition.TOP, Configs.BaseFormControlSpacingHalf, labelColor, labelFont);

            text_license_status = LabeledComponent.StatefulTextbox(pedContent, "License Status", RelationalPosition.LEFT, Configs.BaseFormControlSpacingHalf, labelColor, labelFont);
            text_times_stopped = LabeledComponent.StatefulTextbox(pedContent, "Times Stopped", RelationalPosition.LEFT, Configs.BaseFormControlSpacingHalf, labelColor, labelFont);
            text_wanted_status_false = LabeledComponent.StatefulTextbox(pedContent, "Wanted Status", RelationalPosition.LEFT, Configs.BaseFormControlSpacingHalf, labelColor, labelFont);
            text_wanted_status_true = LabeledComponent.StatefulMultilineTextBox(pedContent, "Wanted Status", RelationalPosition.LEFT, Configs.BaseFormControlSpacingHalf, labelColor, labelFont);

            ped_image_holder = new ImagePanel(pedContent);
            ped_image_holder.SetSize(155, 217);
            ped_image_holder.ImageName = Function.DetermineImagePath(ThePed);
            ped_image_holder.ShouldCacheToTexture = true;

            text_first_name.Component.Disable();
            text_last_name.Component.Disable();
            text_age.Component.Disable();

            text_home_address.Component.Disable();
            text_dob.Component.Disable();

            text_license_status.Component.Disable();
            text_times_stopped.Component.Disable();

            text_wanted_status_false.Component.Disable();
            text_wanted_status_false.Component.Hide();
            text_wanted_status_true.Component.Disable();
            text_wanted_status_true.Component.Hide();

            cb_action.ItemSelected += ActionSelected;
        }

        protected override void Layout(GwenSkin.Base skin)
        {
            base.Layout(skin);

            BindData();
            cb_action.SetSize(200, cb_action.Height);
            cb_action.PlaceLeftOf();
            cb_action.LogPositionAndSize();

            pedInformation
             .AddContentChild(pedContent)
             .PlaceBelowOf(cb_action)
             .AlignLeftWith()
             .SizeWidthWith();

            text_first_name.Component.NormalSize();
            text_last_name.Component.NormalSize();
            text_age.Component.SmallSize();
            text_times_stopped.Component.SmallSize();
            text_wanted_status_false.Component.SmallSize();
            text_wanted_status_true.Component.SetSize(332, 90);
            text_license_status.Component.SetSize(150, 21);
            text_dob.Component.SmallSize();
            text_home_address.Component.LongSize();

            text_last_name
                .PlaceRightOf(text_first_name, Configs.BaseFormControlSpacingDouble)
                .AlignTopWith(text_first_name);

            text_age
                .PlaceRightOf(text_last_name, Configs.BaseFormControlSpacingDouble)
                .AlignTopWith(text_last_name);

            text_home_address
                .PlaceBelowOf(text_first_name, Configs.BaseFormControlSpacingDouble)
                .AlignLeftWith(text_first_name);

            text_dob
                .Align(text_age, text_home_address);

            text_license_status
                .PlaceBelowOf(text_home_address, Configs.BaseFormControlSpacingDouble)
                .AlignLeftWith(text_home_address);

            text_times_stopped
                .PlaceBelowOf(text_license_status)
                .AlignLeftWith(text_license_status);

            text_wanted_status_false
                .PlaceBelowOf(text_times_stopped)
                .AlignLeftWith(text_times_stopped);

            text_wanted_status_true
                .PlaceBelowOf(text_times_stopped)
                .AlignLeftWith(text_times_stopped);

            ped_image_holder
               .PlaceLeftOf();

            pedInformation.SizeToChildrenBlock();
            pedContent.SizeToChildrenBlock();
        }

        private void BindData()
        {
            if (ThePed == null || !ThePed.IsValid()) return;
            if (!BindNeeded) return;
            BindNeeded = false;

            cb_action.AddItem("Select One", "PlaceHolder", QuickActions.PLACEHOLDER);
            if (ThePed.LastVehicle != null) //Not using the implicit bool operator for Vehicle because we dont care if it is "valid" any more, we only care that they "had" a vehicle
                cb_action.AddItem("Create Traffic Citation", "TrafficCitation", QuickActions.CREATE_TRAFFIC_CITATION);
            cb_action.AddItem("Create Arrest Report", "ArrestReport", QuickActions.CREATE_ARREST_REPORT);

            text_first_name.Component.Text = DetailedEntity.Entity.FirstName;
            text_last_name.Component.Text = DetailedEntity.Entity.LastName;
            text_dob.Component.Text = DetailedEntity.Entity.DOBString;
            text_age.Component.Text = DetailedEntity.Entity.AgeString;
            text_home_address.Component.Text = DetailedEntity.Entity.Ped.GetHomeAddress();
            text_times_stopped.Component.Text = DetailedEntity.Entity.TimesStopped.ToString();

            if (DetailedEntity.Entity.IsWanted)
            {
                text_wanted_status_true.Component.IsHidden = false;
                text_wanted_status_false.Component.IsHidden = true;
                text_wanted_status_true.Component.Warn("Wanted for " + DetailedEntity.Entity.WantedReason);
            }
            else
            {
                text_wanted_status_true.Component.IsHidden = true;
                text_wanted_status_false.Component.IsHidden = false;
                text_wanted_status_false.Component.SetText("None");
            }

            if (DetailedEntity.Entity.IsLicenseValid)
            {
                text_license_status.Component.Text = "Valid";
            }
            else
            {
                string licenseStateString = DetailedEntity.Entity.LicenseStateString;
                if (licenseStateString.Equals("Expired"))
                    text_license_status.Component.Warn(String.Format(@"Expired ({0} days)", ThePed.GetDrivingLicenseExpirationDuration()));
                else
                    text_license_status.Component.Warn(licenseStateString);
            }


            /*
            if (Entity == null) return;
            lock (Entity)
            {
                if (Entity.IsLicenseValid)
                {
                    text_license_status.Text = "Valid";
                }
                else
                {
                    string licenseStateString = Entity.LicenseStateString;
                    if (licenseStateString.Equals("Expired"))
                        text_license_status.Warn(String.Format(@"Expired ({0} days)", Entity.Ped.GetDrivingLicenseExpirationDuration()));
                    else
                        text_license_status.Warn(licenseStateString);
                }
                
                if (Entity.IsAgent)
                {
                    lbl_alert.SetText("Federal agent");
                }
                else if (Entity.IsCop)
                {
                    lbl_alert.SetText("LEO");
                }

                text_age.Text = Entity.AgeString;
                text_first_name.Text = Entity.FirstName;
                text_last_name.Text = Entity.LastName;
                text_home_address.Text = Entity.Ped.GetHomeAddress();
                text_dob.Text = Entity.DOBString;
                //text_dob.SetToolTipText("MM/dd/yyyy");

                if (Entity.IsWanted)
                {
                    text_wanted_status_true.IsHidden = false;
                    text_wanted_status_false.IsHidden = true;
                    text_wanted_status_true.Warn("Wanted for " + Entity.WantedReason);
                }
                else
                {
                    text_wanted_status_true.IsHidden = true;
                    text_wanted_status_false.IsHidden = false;
                    text_wanted_status_false.SetText("None");
                }

                text_times_stopped.Text = Entity.TimesStopped.ToString();
            }
            */
        }

        private void ActionSelected(Base sender, ItemSelectedEventArgs arguments)
        {
            if (arguments.SelectedItem == null || (QuickActions)arguments.SelectedItem.UserData == QuickActions.PLACEHOLDER || arguments.SelectedItem.Name.Equals("Placeholder")) return;
            OnQuickActionSelected(this, (QuickActions)arguments.SelectedItem.UserData);
            cb_action.SelectByUserData(QuickActions.PLACEHOLDER);
        }

        private void OnQuickAction(object sender, QuickActions action)
        {
            switch (action)
            {
                case QuickActions.CREATE_TRAFFIC_CITATION:
                    {
                        ComputerReportsController.ShowTrafficCitationCreate(null, DetailedEntity.Entity);
                        return;
                    }
                case QuickActions.CREATE_ARREST_REPORT:
                    {
                        ComputerReportsController.ShowArrestReportCreate(DetailedEntity.Entity, null);
                        return;
                    }
            }
        }

    }
}
