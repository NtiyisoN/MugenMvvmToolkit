package com.mugen.mvvm.views;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Context;
import android.content.ContextWrapper;
import android.content.Intent;
import android.view.View;
import android.widget.Toolbar;
import androidx.appcompat.app.AppCompatActivity;
import com.mugen.mvvm.MugenNativeService;
import com.mugen.mvvm.interfaces.views.IActivityView;
import com.mugen.mvvm.interfaces.views.INativeActivityView;
import com.mugen.mvvm.internal.ActivityAttachedValues;
import com.mugen.mvvm.views.activities.MainMugenActivity;
import com.mugen.mvvm.views.activities.MugenActivity;
import com.mugen.mvvm.views.activities.MainMugenAppCompatActivity;
import com.mugen.mvvm.views.activities.MugenAppCompatActivity;

public abstract class ActivityExtensions {
    private static boolean _isNativeActivityMode;
    static final String ViewIdIntentKey = "~v_id!";

    public static boolean isNativeActivityMode() {
        return _isNativeActivityMode;
    }

    public static void setNativeActivityMode() {
        _isNativeActivityMode = true;
    }

    public static Object getActionBar(Context activity) {
        if (MugenNativeService.isCompatSupported() && activity instanceof AppCompatActivity)
            return ((AppCompatActivity) activity).getSupportActionBar();
        return ((Activity) activity).getActionBar();
    }

    @SuppressLint("NewApi")
    public static boolean setActionBar(Context activityObj, View toolbar) {
        if (ToolbarExtensions.isSupportedCompat(toolbar)) {
            AppCompatActivity activity = (AppCompatActivity) activityObj;
            activity.setSupportActionBar((androidx.appcompat.widget.Toolbar) toolbar);
            return true;
        }
        if (ToolbarExtensions.isSupported(toolbar)) {
            Activity activity = (Activity) activityObj;
            activity.setActionBar((Toolbar) toolbar);
            return true;
        }
        return false;
    }

    public static boolean startActivity(IActivityView activityView, Class activityClass, int resourceId, int flags) {
        if (activityClass == null)
            activityClass = ViewExtensions.tryGetClassById(resourceId);
        if (activityClass == null)
            return false;

        Context context = activityView == null ? MugenNativeService.getAppContext() : activityView.getActivity();
        Intent intent = new Intent(context, activityClass);
        if (activityView == null)
            intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        if (flags != 0)
            intent.addFlags(flags);
        if (resourceId != 0)
            intent.putExtra(ViewIdIntentKey, resourceId);
        context.startActivity(intent);
        return true;
    }

    public static Context getActivity(Context context) {
        while (true) {
            if (context instanceof Activity)
                return context;

            if (context instanceof ContextWrapper) {
                context = ((ContextWrapper) context).getBaseContext();
                continue;
            }
            return null;
        }
    }

    public static Object tryWrapActivity(Object target) {
        if (!isNativeActivityMode())
            return target;
        if (target instanceof INativeActivityView) {
            ActivityAttachedValues attachedValues = (ActivityAttachedValues) ViewExtensions.getNativeAttachedValues(target, true);
            Object wrapper = attachedValues.getWrapper();
            if (wrapper == null) {
                wrapper = new ActivityWrapper((INativeActivityView) target);
                attachedValues.setWrapper(wrapper);
            }
            return wrapper;
        }
        return target;
    }

    public static void setMainActivityMapping(int resource, boolean isCompat) {
        ViewExtensions.addViewMapping(isCompat ? MainMugenAppCompatActivity.class : MainMugenActivity.class, resource);
    }

    public static void addCommonActivityMapping(int resource, boolean isCompat) {
        ViewExtensions.addViewMapping(isCompat ? MugenAppCompatActivity.class : MugenActivity.class, resource);
    }
}