using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PCL.Core.Logging;
using PCL.Core.UI;
using PCL.Core.UI.Animation;
using PCL.Core.UI.Animation.Animatable;
using PCL.Core.UI.Animation.Core;
using PCL.Core.UI.Animation.Easings;
using PCL.Core.Utils;

namespace PCL.Modules.Base;

public static class LegacyAnimationWrapper
{
    public static void Start(string name, ModAnimation.AniGroupEntry entry)
    {
        var ani = HandleLegacyAnimation(entry);
        ani.Name = name;
        
        ani.RunFireAndForget(EmptyAnimatable.Instance);
    }
    
    public static void Stop(string name) => AnimationService.CancelAnimationByName(name);

    public static bool IsRunning(string name) => AnimationService.IsRunningByName(name);

    private static IAnimation HandleLegacyAnimation(ModAnimation.AniGroupEntry entry)
    {
        var data = entry.data;
        if (data.Count == 1)
        {
            return HandleSingleLegacyAnimation(data[0]);
        }
        
        var group = new SequentialAnimationGroup();
        List<IAnimation> animations = [];
        foreach (var ani in data)
        {
            if (ani.isAfter)
            {
                group.Children.Add(new ParallelAnimationGroup(animations));
                animations.Clear();
            }
            
            var animation = HandleSingleLegacyAnimation(ani);
            animations.Add(animation);
        }
        
        group.Children.Add(new ParallelAnimationGroup(animations));
        return group;
    }

    private static IAnimation HandleSingleLegacyAnimation(ModAnimation.AniData data)
    {
        return data.typeMain switch
        {
            ModAnimation.AniType.Number => HandleNumberAnimation(data),
            ModAnimation.AniType.Color => HandleColorAnimation(data),
            ModAnimation.AniType.Code => HandleCodeAnimation(data),
            _ => HandleOtherAnimation(data)
        };

        IAnimation HandleNumberAnimation(ModAnimation.AniData number)
        {
            AnimationBase ani = new DoubleFromToAnimation
            {
                From = 0,
                To = (double)number.value,
                ValueType = AnimationValueType.Relative,
                Duration = TimeSpan.FromMilliseconds(number.timeTotal),
                Delay = TimeSpan.FromMilliseconds(-number.timeFinished),
                Easing = HandleLegacyEasing(number.ease)
            };
            AnimationExtensions.SetTarget(ani, (DependencyObject)number.obj);

            switch (number.typeSub)
            {
                case ModAnimation.AniTypeSub.X:
                    ani = new XFromToAnimation
                    {
                        From = 0,
                        To = (double)number.value,
                        ValueType = AnimationValueType.Relative,
                        Duration = TimeSpan.FromMilliseconds(number.timeTotal),
                        Delay = TimeSpan.FromMilliseconds(-number.timeFinished),
                        Easing = HandleLegacyEasing(number.ease)
                    };
                    AnimationExtensions.SetTarget(ani, (DependencyObject)number.obj);
                    break;
                case ModAnimation.AniTypeSub.Y:
                    ani = new YFromToAnimation
                    {
                        From = 0,
                        To = (double)number.value,
                        ValueType = AnimationValueType.Relative,
                        Duration = TimeSpan.FromMilliseconds(number.timeTotal),
                        Delay = TimeSpan.FromMilliseconds(-number.timeFinished),
                        Easing = HandleLegacyEasing(number.ease)
                    };
                    AnimationExtensions.SetTarget(ani, (DependencyObject)number.obj);
                    break;
                case ModAnimation.AniTypeSub.Opacity:
                    AnimationExtensions.SetTargetProperty(ani, UIElement.OpacityProperty);
                    break;
                case ModAnimation.AniTypeSub.Width:
                    AnimationExtensions.SetTargetProperty(ani, FrameworkElement.WidthProperty);
                    break;
                case ModAnimation.AniTypeSub.Height:
                    AnimationExtensions.SetTargetProperty(ani, FrameworkElement.HeightProperty);
                    break;
                case ModAnimation.AniTypeSub.TranslateX:
                    var transformX =
                        ControlHelper.GetOrCreateTransform<TranslateTransform>((DependencyObject)number.obj);
                    AnimationExtensions.SetTarget(ani, transformX);
                    AnimationExtensions.SetTargetProperty(ani, TranslateTransform.XProperty);
                    break;
                case ModAnimation.AniTypeSub.TranslateY:
                    var transformY =
                        ControlHelper.GetOrCreateTransform<TranslateTransform>((DependencyObject)number.obj);
                    AnimationExtensions.SetTarget(ani, transformY);
                    AnimationExtensions.SetTargetProperty(ani, TranslateTransform.YProperty);
                    break;
                case ModAnimation.AniTypeSub.Double:
                    AnimationExtensions.SetTarget(ani, (DependencyObject)((object[])number.obj)[0]);
                    AnimationExtensions.SetTargetProperty(ani, (DependencyProperty)((object[])number.obj)[1]);
                    break;
                case ModAnimation.AniTypeSub.DoubleParam:
                    // 最傻逼的一种动画方式
                    ani = new DoubleParamFromToAnimation
                    {
                        From = 0,
                        To = (double)number.value,
                        ValueType = AnimationValueType.Relative,
                        Duration = TimeSpan.FromMilliseconds(number.timeTotal),
                        Delay = TimeSpan.FromMilliseconds(-number.timeFinished),
                        Easing = HandleLegacyEasing(number.ease),
                        Param = (ParameterizedThreadStart)number.obj
                    };
                    AnimationExtensions.SetAnimatable(ani, EmptyAnimatable.Instance);
                    break;
                case ModAnimation.AniTypeSub.GridLengthWidth:
                    // 天马行空...
                    ani = new GridLengthFromToAnimation
                    {
                        From = new GridLength(0, GridUnitType.Star),
                        To = new GridLength((double)number.value, GridUnitType.Star),
                        ValueType = AnimationValueType.Relative,
                        Duration = TimeSpan.FromMilliseconds(number.timeTotal),
                        Delay = TimeSpan.FromMilliseconds(-number.timeFinished),
                        Easing = HandleLegacyEasing(number.ease)
                    };
                    AnimationExtensions.SetTarget(ani, (DependencyObject)number.obj);
                    AnimationExtensions.SetTargetProperty(ani, ColumnDefinition.WidthProperty);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return ani;
        }

        NColorFromToAnimation HandleColorAnimation(ModAnimation.AniData color)
        {
            var c = (ModBase.MyColor)color.value;
            var ani = new NColorFromToAnimation
            {
                From = new NColor(),
                To = new NColor((float)c.r, (float)c.g, (float)c.b, (float)c.a),
                ValueType = AnimationValueType.Relative,
                Duration = TimeSpan.FromMilliseconds(color.timeTotal),
                Delay = TimeSpan.FromMilliseconds(-color.timeFinished),
                Easing = HandleLegacyEasing(color.ease)
            };
            AnimationExtensions.SetTarget(ani, (DependencyObject)((object[])color.obj)[0]);
            AnimationExtensions.SetTargetProperty(ani, (DependencyProperty)((object[])color.obj)[1]);

            return ani;
        }

        ActionAnimation HandleCodeAnimation(ModAnimation.AniData code)
        {
            return new ActionAnimation(() => ((ThreadStart)code.value)());
        }

        ActionAnimation HandleOtherAnimation(ModAnimation.AniData other)
        {
            var uuid = ModBase.GetUuid();
            LogWrapper.Debug("Animation", $"出现 LegacyAnimationWrapper 未转换的动画，请及时修改。调用栈：{new StackTrace(skipFrames: 4)}");
            
            return new ActionAnimation(() =>
            {
                ModAnimation.aniGroups.TryAdd($"LegacyAnimation_{uuid}", new ModAnimation.AniGroupEntry
                {
                    data = [other],
                    startTick = TimeUtils.GetTimeTick(),
                    Uuid = uuid
                });
            });
        }
    }

    private static IEasing HandleLegacyEasing(ModAnimation.AniEase ease)
    {
        return ease switch
        {
            ModAnimation.AniEaseInout inout => HandleCombinedEasing(inout),
            ModAnimation.AniEaseLinear => LinearEasing.Shared,
            ModAnimation.AniEaseInFluent f => HandleFluentEasing(f),
            ModAnimation.AniEaseOutFluent f => HandleFluentEasing(f),
            ModAnimation.AniEaseInoutFluent f => HandleFluentEasing(f),
            ModAnimation.AniEaseInBack b => HandleBackEasing(b),
            ModAnimation.AniEaseOutBack b => HandleBackEasing(b),
            _ => CreateCustomEasing(ease)
        };

        CombinedEasing HandleCombinedEasing(ModAnimation.AniEaseInout combined)
        {
            return new CombinedEasing(
                HandleLegacyEasing(combined.easeIn),
                HandleLegacyEasing(combined.easeOut),
                combined.easeInPercent);
        }

        IEasing HandleFluentEasing(ModAnimation.AniEase fluent)
        {
            return fluent switch
            {
                ModAnimation.AniEaseInFluent fIn => fIn.p switch
                {
                    ModAnimation.AniEasePower.Weak => QuadEaseIn.Shared,
                    ModAnimation.AniEasePower.Middle => CubicEaseIn.Shared,
                    ModAnimation.AniEasePower.Strong => QuarticEaseIn.Shared,
                    ModAnimation.AniEasePower.ExtraStrong => QuinticEaseIn.Shared,
                    _ => throw new NotSupportedException($"不支持的缓动能量: {fIn.p}")
                },
                ModAnimation.AniEaseOutFluent fOut => fOut.p switch
                {
                    ModAnimation.AniEasePower.Weak => QuadEaseOut.Shared,
                    ModAnimation.AniEasePower.Middle => CubicEaseOut.Shared,
                    ModAnimation.AniEasePower.Strong => QuarticEaseOut.Shared,
                    ModAnimation.AniEasePower.ExtraStrong => QuinticEaseOut.Shared,
                    _ => throw new NotSupportedException($"不支持的缓动能量: {fOut.p}")
                },
                ModAnimation.AniEaseInoutFluent fInout => fInout.p switch
                {
                    ModAnimation.AniEasePower.Weak => QuadEaseInOut.Shared,
                    ModAnimation.AniEasePower.Middle => CubicEaseInOut.Shared,
                    ModAnimation.AniEasePower.Strong => QuarticEaseInOut.Shared,
                    ModAnimation.AniEasePower.ExtraStrong => QuinticEaseInOut.Shared,
                    _ => throw new NotSupportedException($"不支持的缓动能量: {fInout.p}")
                },
                _ => throw new NotSupportedException($"不支持的缓动类型: {fluent.GetType().Name}")
            };
        }

        IEasing HandleBackEasing(ModAnimation.AniEase back)
        {
            return back switch
            {
                ModAnimation.AniEaseInBack bIn => new BackEaseWithPowerIn(HandleLegacyEasePower(bIn.p)),
                ModAnimation.AniEaseOutBack bOut => new BackEaseWithPowerOut(HandleLegacyEasePower(bOut.p)),
                _ => throw new NotSupportedException($"不支持的缓动类型: {back.GetType().Name}")
            };
        }

        EasePower HandleLegacyEasePower(ModAnimation.AniEasePower easePower)
        {
            return easePower switch
            {
                ModAnimation.AniEasePower.Weak => EasePower.Weak,
                ModAnimation.AniEasePower.Middle => EasePower.Middle,
                ModAnimation.AniEasePower.Strong => EasePower.Strong,
                ModAnimation.AniEasePower.ExtraStrong => EasePower.ExtraStrong,
                _ => throw new NotSupportedException($"不支持的缓动能量: {easePower}")
            };
        }

        CustomEasing CreateCustomEasing(ModAnimation.AniEase e) => new() { EasingFunction = e.GetValue };
    }
}