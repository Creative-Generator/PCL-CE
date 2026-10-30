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

namespace PCL.Modules.Base.Animations;

public static class LegacyAnimationWrapper
{
    public static void Start(string name, PCL.Animations.AniGroupEntry entry)
    {
        var ani = HandleLegacyAnimation(entry);
        ani.Name = name;

        ani.RunFireAndForget(EmptyAnimatable.Instance);
    }

    public static void Stop(string name) => AnimationService.CancelAnimationByName(name);

    public static bool IsRunning(string name) => AnimationService.IsRunningByName(name);

    private static IAnimation HandleLegacyAnimation(PCL.Animations.AniGroupEntry entry)
    {
        var data = entry.data;

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

    private static IAnimation HandleSingleLegacyAnimation(PCL.Animations.AniData data)
    {
        return data.typeMain switch
        {
            PCL.Animations.AniType.Number => HandleNumberAnimation(data),
            PCL.Animations.AniType.Color => HandleColorAnimation(data),
            PCL.Animations.AniType.Code => HandleCodeAnimation(data),
            _ => HandleOtherAnimation(data)
        };

        IAnimation HandleNumberAnimation(PCL.Animations.AniData number)
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

            switch (number.typeSub)
            {
                case PCL.Animations.AniTypeSub.X:
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
                    AnimationExtensions.SetTargetProperty(ani, FrameworkElement.TagProperty);
                    break;
                case PCL.Animations.AniTypeSub.Y:
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
                    AnimationExtensions.SetTargetProperty(ani, FrameworkElement.TagProperty);
                    break;
                case PCL.Animations.AniTypeSub.Opacity:
                    AnimationExtensions.SetTarget(ani, (DependencyObject)number.obj);
                    AnimationExtensions.SetTargetProperty(ani, UIElement.OpacityProperty);
                    break;
                case PCL.Animations.AniTypeSub.Width:
                    AnimationExtensions.SetTarget(ani, (DependencyObject)number.obj);
                    AnimationExtensions.SetTargetProperty(ani, FrameworkElement.WidthProperty);
                    break;
                case PCL.Animations.AniTypeSub.Height:
                    AnimationExtensions.SetTarget(ani, (DependencyObject)number.obj);
                    AnimationExtensions.SetTargetProperty(ani, FrameworkElement.HeightProperty);
                    break;
                case PCL.Animations.AniTypeSub.TranslateX:
                    var transformX =
                        ControlHelper.GetOrCreateTransform<TranslateTransform>((DependencyObject)number.obj);
                    AnimationExtensions.SetTarget(ani, transformX);
                    AnimationExtensions.SetTargetProperty(ani, TranslateTransform.XProperty);
                    break;
                case PCL.Animations.AniTypeSub.TranslateY:
                    var transformY =
                        ControlHelper.GetOrCreateTransform<TranslateTransform>((DependencyObject)number.obj);
                    AnimationExtensions.SetTarget(ani, transformY);
                    AnimationExtensions.SetTargetProperty(ani, TranslateTransform.YProperty);
                    break;
                case PCL.Animations.AniTypeSub.Double:
                    AnimationExtensions.SetTarget(ani, (DependencyObject)((object[])number.obj)[0]);
                    AnimationExtensions.SetTargetProperty(ani, (DependencyProperty)((object[])number.obj)[1]);
                    break;
                case PCL.Animations.AniTypeSub.DoubleParam:
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
                case PCL.Animations.AniTypeSub.GridLengthWidth:
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
                    // 难搞
                    return HandleOtherAnimation(number);
            }

            return ani;
        }

        IAnimation HandleColorAnimation(PCL.Animations.AniData color)
        {
            var c = (Common.MyColor)color.value;
            var objList = (object[])color.obj;
            var reference = (string)objList[2];
            var dependencyProperty = (DependencyProperty)objList[1];
            AnimationBase ani;
            if (!string.IsNullOrEmpty(reference))
            {
                ani = new SequentialAnimationGroup([
                    CreateColorAnimation(),
                    new ActionAnimation(() =>
                        ((FrameworkElement)objList[0]).SetResourceReference(dependencyProperty, reference))
                ]);
            }
            else
            {
                ani = CreateColorAnimation();
            }

            AnimationExtensions.SetTarget(ani, (DependencyObject)objList[0]);
            AnimationExtensions.SetTargetProperty(ani, dependencyProperty);

            return ani;

            NColorFromToAnimation CreateColorAnimation()
            {
                return new NColorFromToAnimation
                {
                    From = new NColor(0, 0, 0, 0),
                    To = new NColor((float)c.r, (float)c.g, (float)c.b, (float)c.a),
                    ValueType = AnimationValueType.Relative,
                    Duration = TimeSpan.FromMilliseconds(color.timeTotal),
                    Delay = TimeSpan.FromMilliseconds(-color.timeFinished),
                    Easing = HandleLegacyEasing(color.ease)
                };
            }
        }

        ActionAnimation HandleCodeAnimation(PCL.Animations.AniData code)
        {
            return new ActionAnimation
            {
                Delay = TimeSpan.FromMilliseconds(-code.timeFinished),
                Action = _ => ((ThreadStart)code.value)()
            };
        }

        ActionAnimation HandleOtherAnimation(PCL.Animations.AniData other)
        {
            var uuid = Common.GetUuid();
            LogWrapper.Debug("Animation",
                $"出现 LegacyAnimationWrapper 未转换的动画，请及时修改。调用栈：{Environment.NewLine}{new StackTrace(skipFrames: 4)}");

            return new ActionAnimation(() => { PCL.Animations.AniStartLegacyCore(other, $"LegacyAnimation_{uuid}"); });
        }
    }

    private static IEasing HandleLegacyEasing(PCL.Animations.AniEase ease)
    {
        return ease switch
        {
            PCL.Animations.AniEaseInout inout => new CombinedEasing(
                HandleLegacyEasing(inout.EaseIn),
                HandleLegacyEasing(inout.EaseOut),
                inout.EaseInPercent),
            PCL.Animations.AniEaseLinear => LinearEasing.Shared,
            PCL.Animations.AniEaseInFluent fluent => HandleFluentEasing(fluent),
            PCL.Animations.AniEaseOutFluent fluent => HandleFluentEasing(fluent),
            PCL.Animations.AniEaseInoutFluent fluent => HandleFluentEasing(fluent),
            PCL.Animations.AniEaseInBack back => HandleBackEasing(back),
            PCL.Animations.AniEaseOutBack back => HandleBackEasing(back),
            _ => CreateCustomEasing(ease)
        };

        IEasing HandleFluentEasing(PCL.Animations.AniEase fluent)
        {
            return fluent switch
            {
                PCL.Animations.AniEaseInFluent easeIn => easeIn.Power switch
                {
                    PCL.Animations.AniEasePower.Weak => QuadEaseIn.Shared,
                    PCL.Animations.AniEasePower.Middle => CubicEaseIn.Shared,
                    PCL.Animations.AniEasePower.Strong => QuarticEaseIn.Shared,
                    PCL.Animations.AniEasePower.ExtraStrong => QuinticEaseIn.Shared,
                    _ => CreateCustomEasing(fluent)
                },
                PCL.Animations.AniEaseOutFluent easeOut => easeOut.Power switch
                {
                    PCL.Animations.AniEasePower.Weak => QuadEaseOut.Shared,
                    PCL.Animations.AniEasePower.Middle => CubicEaseOut.Shared,
                    PCL.Animations.AniEasePower.Strong => QuarticEaseOut.Shared,
                    PCL.Animations.AniEasePower.ExtraStrong => QuinticEaseOut.Shared,
                    _ => CreateCustomEasing(fluent)
                },
                PCL.Animations.AniEaseInoutFluent easeInout => easeInout.Power switch
                {
                    PCL.Animations.AniEasePower.Weak => QuadEaseInOut.Shared,
                    PCL.Animations.AniEasePower.Middle => CubicEaseInOut.Shared,
                    PCL.Animations.AniEasePower.Strong => QuarticEaseInOut.Shared,
                    PCL.Animations.AniEasePower.ExtraStrong => QuinticEaseInOut.Shared,
                    _ => CreateCustomEasing(fluent)
                },
                _ => CreateCustomEasing(fluent)
            };
        }

        IEasing HandleBackEasing(PCL.Animations.AniEase back)
        {
            return back switch
            {
                PCL.Animations.AniEaseInBack easeIn => new BackEaseWithPowerIn(HandleLegacyEasePower(easeIn.Power)),
                PCL.Animations.AniEaseOutBack easeOut => new BackEaseWithPowerOut(HandleLegacyEasePower(easeOut.Power)),
                _ => CreateCustomEasing(back)
            };
        }

        EasePower HandleLegacyEasePower(PCL.Animations.AniEasePower power)
        {
            return power switch
            {
                PCL.Animations.AniEasePower.Weak => EasePower.Weak,
                PCL.Animations.AniEasePower.Middle => EasePower.Middle,
                PCL.Animations.AniEasePower.Strong => EasePower.Strong,
                PCL.Animations.AniEasePower.ExtraStrong => EasePower.ExtraStrong,
                _ => throw new NotSupportedException($"不支持的缓动能量: {power}")
            };
        }

        CustomEasing CreateCustomEasing(PCL.Animations.AniEase e) => new() { EasingFunction = e.GetValue };
    }
}