using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace REghZy.MVVM.ViewModels {
    /// <summary>
    /// A class that helps with binding properties between view models
    /// </summary>
    public static class Binder {
        // private static readonly Dictionary<BaseViewModel, Dictionary<BaseViewModel, BinderInfo>> BINDERS;
        private static readonly object BINDER_LOCK = new object();
        private static BaseViewModel TARGET;
        private static string TARGET_PROPERTY;

        // static Binder() {
        //     BINDERS = new Dictionary<BaseViewModel, Dictionary<BaseViewModel, BinderInfo>>();
        // }

        // private static Dictionary<BaseViewModel, BinderInfo> GetBindersForFrom(BaseViewModel from) {
        //     Dictionary<BaseViewModel, BinderInfo> dictionary;
        //     if (!BINDERS.TryGetValue(from, out dictionary)) {
        //         BINDERS[from] = dictionary = new Dictionary<BaseViewModel, BinderInfo>();
        //     }
        //     return dictionary;
        // }

        private static BinderInfo CreateBinder(BaseViewModel obj, string property, BaseViewModel target, string targetProperty) {
            BinderInfo info = new BinderInfo(obj, property, target, targetProperty);
            // if (!GetBindersForFrom(obj).TryGetValue(obj, out info)) {
            //     GetBindersForFrom(obj)[target] = info = new BinderInfo(obj, property, target, targetProperty);
            // }

            return info;
        }

        // private static BinderInfo GetBinder(BaseViewModel from, BaseViewModel to) {
        //     if (BINDERS.TryGetValue(from, out Dictionary<BaseViewModel, BinderInfo> dictionary)) {
        //         if (dictionary.TryGetValue(to, out BinderInfo binder)) {
        //             return binder;
        //         }
        //     }
        //     return null;
        // }

        /// <summary>
        /// Binds a property from 'fromObj', and sets the property in 'toObj'
        /// <para>
        /// Binding both ways is supported
        /// </para>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="property"></param>
        /// <param name="targetObj"></param>
        /// <param name="targetProperty"></param>
        public static void Bind(BaseViewModel obj, string property, BaseViewModel targetObj, string targetProperty) {
            // PropertyInfo from = fromObj.GetType().GetProperty(fromProperty);
            // if (from == null) {
            //     throw new MissingMemberException(fromObj.GetType().Name, fromProperty);
            // }
            // PropertyInfo to = toObj.GetType().GetProperty(toProperty);
            // if (to == null) {
            //     throw new MissingMemberException(toObj.GetType().Name, toProperty);
            // }
            // fromObj.PropertyChanged += (sender, args) => {
            //     if (args.PropertyName == fromProperty) {
            //         to.SetValue(toObj, from.GetValue(fromObj));
            //     }
            // };

            BinderInfo info = CreateBinder(obj, property, targetObj, targetProperty);
            obj.PropertyChanged += (sender, args) => {
                if (args.PropertyName == property) {
                    info.Set();
                }
            };
        }

        private class BinderInfo {
            private readonly BaseViewModel obj;
            private readonly BaseViewModel target;
            private readonly string property;
            private readonly string targetProperty;

            private readonly Expression getProperty;
            private readonly Expression getTargetProperty;
            private readonly Action setTarget;

            private bool isBeingSet;
            // private BinderInfo flipped;

            public BinderInfo(BaseViewModel obj, string property, BaseViewModel target, string targetProperty) {
                this.obj = obj;
                this.property = property;
                this.target = target;
                this.targetProperty = targetProperty;
                this.getProperty = Expression.Property(Expression.Constant(this.obj), this.property);
                this.getTargetProperty = Expression.Property(Expression.Constant(this.target), this.targetProperty);
                this.setTarget = Expression.Lambda<Action>(Expression.Assign(this.getTargetProperty, this.getProperty)).Compile();
                // this.flipped = GetBinder(target, obj);
            }

            public void Set() {
                if (this.isBeingSet) {
                    return;
                }

                if (TARGET == this.obj && TARGET_PROPERTY == this.property) {
                    return;
                }

                lock (BINDER_LOCK) {
                    TARGET = this.obj;
                    TARGET_PROPERTY = this.property;
                    this.isBeingSet = true;
                    this.setTarget();
                    this.isBeingSet = false;
                    TARGET = null;
                    TARGET_PROPERTY = null;
                }
            }
        }
    }
}