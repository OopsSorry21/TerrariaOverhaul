﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Interface;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public class SettingsPanel : UIElement
{
	private readonly List<ConfigEntryElement> entries = new();

	// Rows
	public UIGrid OptionRowsGrid { get; }
	public UIElement OptionRowsContainer { get; }
	public FancyUIPanel OptionRowsGridContainer { get; }
	public UIScrollbar OptionRowsScrollbar { get; }
	// Bottom Panel
	public FancyUIPanel BottomPanel { get; }
	// Bottom Panel - Description
	public UIText DescriptionText { get; }
	// Bottom Panel - Icon
	public UIElement OptionIconContainer { get; }
	public UIImage SelectedIconBorder { get; }
	public UIConfigIcon SelectedIconImage { get; }

	public IReadOnlyList<ConfigEntryElement> OptionElements => entries;

	public SettingsPanel() : base()
	{
		// Self

		Width = StyleDimension.Fill;
		Height = StyleDimension.Fill;

		// Main

		OptionRowsContainer = this.AddElement(new UIElement().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.FromPercent(0.7f);
		}));

		OptionRowsGridContainer = OptionRowsContainer.AddElement(new FancyUIPanel().With(e => {
			e.Width = StyleDimension.FromPixelsAndPercent(-32f, 1f);
			e.Height = StyleDimension.Fill;

			e.Colors.CopyFrom(CommonColors.OuterPanelMedium);
		}));

		OptionRowsGrid = OptionRowsGridContainer.AddElement(new UIGrid().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.Fill;
			e.ListPadding = 6f;
		}));

		OptionRowsScrollbar = OptionRowsContainer.AddElement(new UIScrollbar().With(e => {
			e.HAlign = 1f;
			e.VAlign = 0.5f;
			e.Height = StyleDimension.FromPixelsAndPercent(-8f, 1f);

			OptionRowsGrid.SetScrollbar(e);
			OptionRowsGrid.AddComponent(new ScrollbarListenerUIComponent { Scrollbar = e, });
		}));

		// Bottom panel

		BottomPanel = this.AddElement(new FancyUIPanel().With(e => {
			e.Width = StyleDimension.Fill;
			e.Height = StyleDimension.FromPixelsAndPercent(-12f, 0.3f);
			e.HAlign = 0.5f;
			e.VAlign = 1f;

			e.Colors.CopyFrom(CommonColors.OuterPanelBright);
		}));

		// Bottom panel - Description

		DescriptionText = BottomPanel.AddElement(new UIText(LocalizedText.Empty, textScale: 0.9f).With(e => {
			e.IsWrapped = true;
			(e.HAlign, e.VAlign) = (0.0f, 0.0f);
			(e.PaddingLeft, e.PaddingTop) = (116f, 8f);
			(e.TextOriginX, e.TextOriginY) = (0.0f, 0.0f);
			e.MaxWidth = e.Width = new StyleDimension(4f, 1f);
		}));

		// Bottom Panel - Icon

		OptionIconContainer = BottomPanel.AddElement(new UIElement().With(e => {
			e.Height = StyleDimension.FromPixels(112f);
			e.Width = StyleDimension.FromPixels(112f);
			e.VAlign = 0.5f;
		}));

		SelectedIconImage = OptionIconContainer.AddElement(new UIConfigIcon(CommonAssets.UnknownOptionTexture, CommonAssets.BackgroundTextures[0]).With(e => {
			e.MaxWidth = e.Width = StyleDimension.FromPixelsAndPercent(-6f, 1.0f);
			e.MaxHeight = e.Height = StyleDimension.FromPixelsAndPercent(-6f, 1.0f);

			e.HAlign = 0.5f;
			e.VAlign = 0.5f;
		}));

		SelectedIconBorder = OptionIconContainer.AddElement(new UIImage(CommonAssets.UnselectedIconBorderTexture).With(e => {
			e.HAlign = 0.5f;
			e.VAlign = 0.5f;
		}));

		// Final

		ResetDescription();
	}

	public void AddOption(IConfigEntry configEntry)
	{
		var entryElement = new ConfigEntryElement(configEntry).With(e => {
			e.CollisionExtents = new(3, 3, 3, 4);

			e.OnMouseOver += (_, element) => UpdateDescription((ConfigEntryElement)element);
			e.OnMouseOut += (_, _) => ResetDescription();
		});

		entries.Add(entryElement);
		OptionRowsGrid.Add(entryElement);
	}

	private void UpdateDescription(ConfigEntryElement element)
	{
		// Text
		var description = element.Description;

		DescriptionText.SetText(description, 1.0f, false);
		DescriptionText.Recalculate();

		var textDimensions = DescriptionText.GetDimensions();
		var panelDimensions = BottomPanel.GetDimensions();

		if (textDimensions.Height >= panelDimensions.Height) {
			float scale = Math.Max(0.75f, panelDimensions.Height / textDimensions.Height);

			DescriptionText.SetText(description, scale, false);
		}

		// Icon
		if (element.IconTexture != null) {
			const float BaseResolution = 64f;
			const float MinOutlineSize = 1f;

			SelectedIconImage.ForegroundTexture = element.IconTexture;
			SelectedIconImage.OutlineSize = Vector2.Max(Vector2.One * MinOutlineSize, element.IconTexture.Size() / (Vector2.One * BaseResolution));
		}

		SelectedIconImage.BackgroundTexture = CommonAssets.GetBackgroundTexture(Math.Abs(element.ConfigEntry.Name.GetHashCode()));

		Recalculate();
	}

	private void ResetDescription()
	{
		// Text
		var descriptionTip = Language.GetText($"Mods.{nameof(TerrariaOverhaul)}.Configuration.HoverForDescriptionTip");

		DescriptionText.SetText($"[c/{Color.LightGoldenrodYellow.ToHexRGB()}:{descriptionTip}]", 1.0f, false);

		// Icon
		SelectedIconImage.ForegroundTexture = CommonAssets.UnknownOptionTexture;
		SelectedIconImage.BackgroundTexture = CommonAssets.GetBackgroundTexture(Main.rand.Next(int.MaxValue));
		SelectedIconImage.OutlineSize = Vector2.One;

		Recalculate();
	}
}
