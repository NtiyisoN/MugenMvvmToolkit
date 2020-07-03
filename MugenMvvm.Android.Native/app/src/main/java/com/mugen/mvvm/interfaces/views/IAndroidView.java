package com.mugen.mvvm.interfaces.views;

import android.view.View;
import com.mugen.mvvm.interfaces.IMemberObserver;

public interface IAndroidView extends IResourceView, IHasTagView, IReleasable {
    View getView();

    Object getParent();

    void setParent(Object parent);

    Object findRelativeSource(String name, int level);

    Object getElementById(int id);

    int getVisibility();

    void setVisibility(int visibility);

    void setBackgroundColor(int color);

    void addMemberListener(String memberName);

    void removeMemberListener(String memberName);

    IMemberObserver getMemberObserver();

    void setMemberObserver(IMemberObserver listener);

    void onMemberChanged(String member, Object args);
}