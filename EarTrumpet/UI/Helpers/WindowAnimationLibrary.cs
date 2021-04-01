﻿using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Views;
using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace EarTrumpet.UI.Helpers
{
    public class WindowAnimationLibrary
    {
        const int _animationOffset = 25;
        private static Storyboard bannerStoryBoard;

        public static void StopAnimation(Window window)
        {
            if (bannerStoryBoard != null)
            {
                bannerStoryBoard.Stop(window);
            }
        }

        public static void BeginBannerExitAnimation(Window window)
        {
            if (SystemSettings.IsTransparencyEnabled)
            {
                var fadeAnimation = new DoubleAnimation
                {
                    Duration = new Duration(TimeSpan.FromMilliseconds(VolumeBanner.FadeTime)),
                    FillBehavior = FillBehavior.Stop,
                    EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
                    From = 1.0,
                    To = 0.0,
                };
                fadeAnimation.Completed += (s, e) => { window.Opacity = 0.0; };

                Storyboard.SetTarget(fadeAnimation, window);
                Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));

                bannerStoryBoard = new Storyboard();
                bannerStoryBoard.FillBehavior = FillBehavior.Stop;

                bannerStoryBoard.Children.Add(fadeAnimation);
                bannerStoryBoard.Completed += new EventHandler((s, e) => { window.Hide(); });
                bannerStoryBoard.Begin(window, true);
            }
            else
            {
                window.Hide();
            }
        }

        public static void BeginFlyoutEntranceAnimation(Window window, WindowsTaskbar.State taskbar, Action completed)
        {
            var onCompleted = new EventHandler((s, e) =>
            {
                window.Topmost = true;
                window.Focus();
                completed();
            });

            window.Topmost = false;
            window.Activate();

            if (!SystemParameters.MenuAnimation)
            {
                window.Cloak(false);
                onCompleted(null, null);
                return;
            }

            var moveAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(266)),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
            };

            var fadeAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(266)),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
                From = 0.5,
                To = 1
            };
            fadeAnimation.Completed += (s, e) => { window.Opacity = 1; };
            Storyboard.SetTarget(fadeAnimation, window);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(Window.OpacityProperty));

            switch (taskbar.Location)
            {
                case WindowsTaskbar.Position.Left:
                    moveAnimation.To = window.Left;
                    window.Left -= _animationOffset;
                    break;
                case WindowsTaskbar.Position.Right:
                    moveAnimation.To = window.Left;
                    window.Left += _animationOffset;
                    break;
                case WindowsTaskbar.Position.Top:
                    moveAnimation.To = window.Top;
                    window.Top -= _animationOffset;
                    break;
                case WindowsTaskbar.Position.Bottom:
                default:
                    moveAnimation.To = window.Top;
                    window.Top += _animationOffset;
                    break;
            }

            if (taskbar.Location == WindowsTaskbar.Position.Left || taskbar.Location == WindowsTaskbar.Position.Right)
            {
                Storyboard.SetTarget(moveAnimation, window);
                Storyboard.SetTargetProperty(moveAnimation, new PropertyPath(Window.LeftProperty));
                moveAnimation.From = window.Left;
            }
            else
            {
                Storyboard.SetTarget(moveAnimation, window);
                Storyboard.SetTargetProperty(moveAnimation, new PropertyPath(Window.TopProperty));
                moveAnimation.From = window.Top;
            }

            if (SystemSettings.IsTransparencyEnabled)
            {
                window.Opacity = 0.5;
            }

            window.Cloak(false);

            var storyboard = new Storyboard();
            storyboard.FillBehavior = FillBehavior.Stop;
            storyboard.Children.Add(moveAnimation);

            if (SystemSettings.IsTransparencyEnabled)
            {
                storyboard.Children.Add(fadeAnimation);
            }

            storyboard.Completed += onCompleted;
            storyboard.Begin(window);
        }

        public static void BeginFlyoutExitanimation(Window window, Action completed)
        {
            var onCompleted = new EventHandler((s, e) =>
            {
                window.Cloak();
                completed();
            });

            window.Topmost = false;

            if (!SystemParameters.MenuAnimation)
            {
                onCompleted(null, null);
                return;
            }

            var moveAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
            };

            var fadeAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
                From = 1,
                To = 0.75,
            };
            fadeAnimation.Completed += (s, e) => { window.Opacity = 0; };

            Storyboard.SetTarget(fadeAnimation, window);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));

            var taskbarPosition = WindowsTaskbar.Current.Location;

            switch (taskbarPosition)
            {
                case WindowsTaskbar.Position.Left:
                    moveAnimation.To = window.Left - _animationOffset;
                    break;
                case WindowsTaskbar.Position.Right:
                    moveAnimation.To = window.Left - _animationOffset;
                    break;
                case WindowsTaskbar.Position.Top:
                    moveAnimation.To = window.Top - _animationOffset;
                    break;
                case WindowsTaskbar.Position.Bottom:
                default:
                    moveAnimation.To = window.Top + _animationOffset;
                    break;
            }

            if (taskbarPosition == WindowsTaskbar.Position.Left || taskbarPosition == WindowsTaskbar.Position.Right)
            {
                Storyboard.SetTarget(moveAnimation, window);
                Storyboard.SetTargetProperty(moveAnimation, new PropertyPath(Window.LeftProperty));
                moveAnimation.From = window.Left;
            }
            else
            {
                Storyboard.SetTarget(moveAnimation, window);
                Storyboard.SetTargetProperty(moveAnimation, new PropertyPath(Window.TopProperty));
                moveAnimation.From = window.Top;
            }

            var storyboard = new Storyboard();
            storyboard.FillBehavior = FillBehavior.Stop;
            storyboard.Children.Add(moveAnimation);

            if (SystemSettings.IsTransparencyEnabled)
            {
                storyboard.Children.Add(fadeAnimation);
            }

            storyboard.Completed += onCompleted;
            storyboard.Begin(window);
        }

        public static void BeginWindowExitAnimation(Window window, Action completed)
        {
            var onCompleted = new EventHandler((s, e) =>
            {
                window.Cloak();
                completed();
            });

            if (!SystemParameters.MenuAnimation || !SystemSettings.IsTransparencyEnabled)
            {
                window.Dispatcher.BeginInvoke((Action)(() =>
                {
                    onCompleted(null, null);
                }));
            }
            else
            {
                var fadeAnimation = new DoubleAnimation
                {
                    Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                    FillBehavior = FillBehavior.Stop,
                    EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
                    From = 1,
                    To = 0.75,
                };
                fadeAnimation.Completed += (s, e) => { window.Opacity = 0; };

                Storyboard.SetTarget(fadeAnimation, window);
                Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(Window.OpacityProperty));

                var storyboard = new Storyboard();
                storyboard.FillBehavior = FillBehavior.Stop;
                storyboard.Children.Add(fadeAnimation);
                storyboard.Completed += onCompleted;
                storyboard.Begin(window);
            }
        }

        public static void BeginWindowEntranceAnimation(Window window, Action completed, double fromOpacity = 0.5)
        {
            var onCompleted = new EventHandler((s, e) =>
            {
                completed();
            });

            if (!SystemParameters.MenuAnimation)
            {
                window.Cloak(false);
                onCompleted(null, null);
                return;
            }

            var fadeAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(250)),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
                From = fromOpacity,
                To = 1,
            };
            fadeAnimation.Completed += (s, e) => { window.Opacity = 1; };

            Storyboard.SetTarget(fadeAnimation, window);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(Window.OpacityProperty));

            var storyboard = new Storyboard();
            storyboard.FillBehavior = FillBehavior.Stop;
            storyboard.Children.Add(fadeAnimation);
            storyboard.Completed += onCompleted;

            window.Cloak(false);

            storyboard.Begin(window);
        }
    }
}
