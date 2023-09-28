﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osuTK;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Game.Configuration;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// Display the specified mod at a fixed size.
    /// </summary>
    public partial class ModIcon : Container, IHasTooltip
    {
        public readonly BindableBool Selected = new BindableBool();

        private SpriteIcon modIcon = null!;
        private SpriteText modAcronym = null!;
        private SpriteIcon background = null!;

        private const float size = 80;

        public virtual LocalisableString TooltipText => showTooltip ? ((mod as Mod)?.IconTooltip ?? mod.Name) : string.Empty;

        private IMod mod;

        private readonly bool showTooltip;
        private readonly bool showExtendedInformation;

        public IMod Mod
        {
            get => mod;
            set
            {
                if (mod == value)
                    return;

                mod = value;

                if (IsLoaded)
                    updateMod(value);
            }
        }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private Color4 backgroundColour;

        private Sprite extendedBackground = null!;

        private OsuSpriteText extendedText = null!;

        private Container extendedContent = null!;

        private ModSettingChangeTracker? modSettingsChangeTracker;

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="mod">The mod to be displayed</param>
        /// <param name="showTooltip">Whether a tooltip describing the mod should display on hover.</param>
        /// <param name="showExtendedInformation">Whether to display a mod's extended information, if available.</param>
        public ModIcon(IMod mod, bool showTooltip = true, bool showExtendedInformation = true)
        {
            AutoSizeAxes = Axes.X;
            Height = size;

            this.mod = mod ?? throw new ArgumentNullException(nameof(mod));
            this.showTooltip = showTooltip;
            this.showExtendedInformation = showExtendedInformation;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Children = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Name = "main content",
                    Size = new Vector2(size),
                    Children = new Drawable[]
                    {
                        background = new SpriteIcon
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Size = new Vector2(size),
                            Icon = OsuIcon.ModBg,
                            Shadow = true,
                        },
                        modAcronym = new OsuSpriteText
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Colour = OsuColour.Gray(84),
                            Alpha = 0,
                            Font = OsuFont.Numeric.With(null, 22f),
                            UseFullGlyphHeight = false,
                            Text = mod.Acronym
                        },
                        modIcon = new SpriteIcon
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Colour = OsuColour.Gray(84),
                            Size = new Vector2(45),
                            Icon = FontAwesome.Solid.Question
                        },
                    }
                },
                extendedContent = new Container
                {
                    Name = "extended content",
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Size = new Vector2(120, 55),
                    X = size - 22,
                    Children = new Drawable[]
                    {
                        extendedBackground = new Sprite
                        {
                            Texture = textures.Get("Icons/BeatmapDetails/mod-icon-extender"),
                            Size = new Vector2(120, 55),
                        },
                        extendedText = new OsuSpriteText
                        {
                            Font = OsuFont.Default.With(size: 34f, weight: FontWeight.Bold),
                            UseFullGlyphHeight = false,
                            Text = mod.ExtendedIconInformation,
                            X = 5,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Selected.BindValueChanged(_ => updateColour());

            updateMod(mod);
        }

        private void updateMod(IMod value)
        {
            modSettingsChangeTracker?.Dispose();

            if (value is Mod actualMod)
            {
                modSettingsChangeTracker = new ModSettingChangeTracker(new[] { actualMod });
                modSettingsChangeTracker.SettingChanged = _ => updateMod(actualMod);
            }

            modAcronym.Text = value.Acronym;
            modIcon.Icon = value.Icon ?? FontAwesome.Solid.Question;

            if (value.Icon is null)
            {
                modIcon.FadeOut();
                modAcronym.FadeIn();
            }
            else
            {
                modIcon.FadeIn();
                modAcronym.FadeOut();
            }

            backgroundColour = colours.ForModType(value.Type);
            updateColour();

            bool showExtended = showExtendedInformation && !string.IsNullOrEmpty(mod.ExtendedIconInformation);

            extendedContent.Alpha = showExtended ? 1 : 0;

            extendedText.Text = mod.ExtendedIconInformation;
        }

        private void updateColour()
        {
            extendedText.Colour = background.Colour = Selected.Value ? backgroundColour.Lighten(0.2f) : backgroundColour;
            extendedBackground.Colour = Selected.Value ? backgroundColour.Darken(2.4f) : backgroundColour.Darken(2.8f);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            modSettingsChangeTracker?.Dispose();
        }
    }
}
