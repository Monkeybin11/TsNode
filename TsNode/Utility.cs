﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using TsNode.Interface;

namespace TsNode
{
    internal static class VisualTreeExtensions
    {
        public static TParent FindVisualParentWithType<TParent>(this DependencyObject childElement)
            where TParent : class
        {
            FrameworkElement parentElement = (FrameworkElement)VisualTreeHelper.GetParent(childElement);
            if (parentElement != null)
            {
                TParent parent = parentElement as TParent;
                if (parent != null)
                {
                    return parent;
                }

                return FindVisualParentWithType<TParent>(parentElement);
            }

            return null;
        }

        public static IEnumerable<TChild> FindVisualChildrenWithType<TChild>(this DependencyObject root) 
            where TChild : FrameworkElement
        {
            var children = Enumerable.Range(0, VisualTreeHelper.GetChildrenCount(root)).Select(x => VisualTreeHelper.GetChild(root, x)).ToArray();

            foreach (var child in children)
            {
                if (child is TChild t)
                    yield return t;

                foreach (var _ in child.FindVisualChildrenWithType<TChild>())
                    yield return _;
            }
        }

        public static T FindTemplate<T>(this Control root , string name) 
            where T : Visual
        {
            var template = root.Template.FindName(name, root);
            Debug.Assert(template != null, $"{name}が実装されていません。");

            var result = template as T;
            Debug.Assert(result != null, $"{name}は{typeof(T).Name}で実装する必要があります。");

            return result;
        }

        public static T FindChildWithName<T>(this FrameworkElement root, string name) 
            where T : FrameworkElement
        {
            return root.FindChild<T>(x => x.Name == name);
        }

        public static T FindChildWithDataContext<T>(this FrameworkElement root, object datContext) 
            where T : FrameworkElement
        {
            return root.FindChild<T>(x=>x.DataContext == datContext);
        }

        public static T FindChild<T>(this FrameworkElement root, Func<FrameworkElement, bool> compare) 
            where T : FrameworkElement
        {
            var children = Enumerable.Range(0, VisualTreeHelper.GetChildrenCount(root)).Select(x => VisualTreeHelper.GetChild(root, x)).OfType<FrameworkElement>().ToArray();

            foreach (var child in children)
            {
                if (child is T t && compare(child))
                    return t;

                t = child.FindChild<T>(compare);
                if (t != null)
                    return t;
            }
            return null;
        }

        public static bool ContainChildren(this FrameworkElement root, FrameworkElement @object)
        {
            if (@object is null)
                return false;

            if (root == @object)
                return true;

            var children = Enumerable.Range(0, VisualTreeHelper.GetChildrenCount(root)).Select(x => VisualTreeHelper.GetChild(root, x)).OfType<FrameworkElement>().ToArray();

            foreach (var child in children)
            {
                if (child == @object)
                    return true;

                if (child.ContainChildren(@object))
                    return true;
            }
            return false;
        }

        public static bool HitTestCircle(this Visual root, Point center , double radius)
        {
            var result = false;
            VisualTreeHelper.HitTest(root, null, 
                _ => 
                {
                    result = true; return HitTestResultBehavior.Stop;
                }, 
                new GeometryHitTestParameters(new EllipseGeometry(center,radius,radius)));
            return result;
        }

        public static bool HitTestRect(this Visual root, Point center, double width , double height)
        {
            var result = false;
            var rect = new Rect(center.X - width / 2, center.Y - height / 2, width, height);
            VisualTreeHelper.HitTest(root, null, 
                _ => 
                {
                    result = true; return HitTestResultBehavior.Stop;
                }, 
                new GeometryHitTestParameters(new RectangleGeometry(rect)));
            return result;
        }

        public static bool HitTestRect(this Visual root, Rect rect)
        {
            var result = false;
            VisualTreeHelper.HitTest(root, null,
                _ =>
                {
                    result = true; return HitTestResultBehavior.Stop;
                },
                new GeometryHitTestParameters(new RectangleGeometry(rect)));
            return result;
        }

        public static IEnumerable<T> GetHitTestChildrenWithRect<T>(this Visual root, Rect rect) where T : Visual
        {
            var result = new List<DependencyObject>();
            VisualTreeHelper.HitTest(root, null,
                x =>
                {
                    result.Add(x.VisualHit);
                    return HitTestResultBehavior.Continue;
                },
                new GeometryHitTestParameters(new RectangleGeometry(rect)));

            return result.Select(x => x.FindVisualParentWithType<T>())
                .Where(x => x != null)
                .Distinct();
        }
    }

    internal static class ItemsControlEx
    {
        public static IEnumerable<T> GetAsContentControl<T>(this ItemsControl self) where T : FrameworkElement
        {
           return Enumerable.Range(0, self.Items.Count)
                .Select(x => self.Items.GetItemAt(x))
                .OfType<T>();
        }

        public static IEnumerable<ListBoxItem> GetListBoxItems(this ListBox self)
        {
            return GetAsContentControl<ListBoxItem>(self);
        }

        public static IEnumerable<object> EnumerateItems(this ItemsControl self)
        {
            return Enumerable.Range(0, self.Items.Count)
                .Select(x => self.Items.GetItemAt(x));
        }
    }

    internal static class FreezableEx
    {
        public static T DoFreeze<T>(this T _this) where T : Freezable
        {
            if (_this.CanFreeze & _this.IsFrozen is false)
                _this.Freeze();
            return _this;
        }
    }

    internal static class DispathcerObjectEx
    {
        public static void BeginInvoke(this DispatcherObject _this, Action action , DispatcherPriority priority = DispatcherPriority.Normal)
        {
            _this?.Dispatcher?.BeginInvoke(priority, action);
        }
    }

    internal static class MathUtil
    {
        public static Point ToPoint(this Vector vector)
        {
            return new Point(vector.X,vector.Y);
        }

        public static Vector ToVector(this Point point)
        {
            return new Vector(point.X, point.Y);
        }
        public static double Clamp(this double value , double min , double max)
        {
            return Math.Max(min, Math.Min(value, max));
        }

    }

    internal static class SelectUtility
    {
        public static ISelectable[] AddSelect(IEnumerable<ISelectable> allItems, IEnumerable<ISelectable> targetItems)
        {
            var result = new List<ISelectable>();
            foreach (var i in targetItems)
            {
                if (i.IsSelected is false)
                    result.Add(i);
                i.IsSelected = true;
            }
            return result.ToArray();
        }

        public static ISelectable[] ToggleSelect(IEnumerable<ISelectable> allItems, IEnumerable<ISelectable> targetItems)
        {
            var itemsArray = targetItems as ISelectable[] ?? targetItems.ToArray();
            foreach (var i in itemsArray)
                i.IsSelected = !i.IsSelected;
            return itemsArray;
        }

        public static ISelectable[] SingleSelect(IEnumerable<ISelectable> allItems, IEnumerable<ISelectable> targetItems)
        {
            var result = new List<ISelectable>();

            var targetItemsArray = targetItems.ToArray();

            if (targetItemsArray.Any(x=>x.IsSelected) is false)
            {
                foreach (var i in allItems)
                {
                    if (i.IsSelected)
                        result.Add(i);
                    i.IsSelected = false;
                }
            }

            foreach (var i in targetItemsArray)
            {
                if (i.IsSelected is false)
                    result.Add(i);
                i.IsSelected = true;
            }
            return result.ToArray();
        }

        public static ISelectable[] OnlySelect(IEnumerable<ISelectable> allItems, IEnumerable<ISelectable> targetItems)
        {
            var result = new List<ISelectable>();

            var targetItemsArray = targetItems.ToArray();

            foreach (var i in allItems)
            {
                bool flag = i.IsSelected;
                if (targetItemsArray.Contains(i))
                    i.IsSelected = true;
                else
                    i.IsSelected = false;

                if (flag != i.IsSelected)
                    result.Add(i);
            }
            return result.ToArray();
        }
    }

    internal static class SelectableExtensions
    {
        public static ISelectable[] ToSelectable<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.OfType<ISelectable>().ToArray();
        }

        public static ISelectable[] ToSelectableDataContext(this IEnumerable<FrameworkElement> enumerable)
        {
            return enumerable.Select(x=>x.DataContext).OfType<ISelectable>().ToArray();
        }
    }
}
