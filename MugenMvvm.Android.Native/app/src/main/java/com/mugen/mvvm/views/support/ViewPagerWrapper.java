package com.mugen.mvvm.views.support;

import android.view.View;
import androidx.viewpager.widget.PagerAdapter;
import androidx.viewpager.widget.ViewPager;
import com.mugen.mvvm.interfaces.IContentItemsSourceProvider;
import com.mugen.mvvm.interfaces.IItemsSourceProviderBase;
import com.mugen.mvvm.interfaces.views.IViewPager;
import com.mugen.mvvm.internal.NativeContentItemsSourceProviderWrapper;
import com.mugen.mvvm.internal.ViewParentObserver;
import com.mugen.mvvm.internal.support.MugenPagerAdapter;
import com.mugen.mvvm.views.ViewWrapper;

public class ViewPagerWrapper extends ViewWrapper implements IViewPager {
    private short _selectedIndexChangedCount;
    private ViewPager.OnPageChangeListener _listener;

    public ViewPagerWrapper(Object view) {
        super(view);
        ViewParentObserver.Instance.remove((View) view, true);
    }

    @Override
    public int getProviderType() {
        return ContentProviderType;
    }

    @Override
    public IItemsSourceProviderBase getItemsSourceProvider() {
        ViewPager view = (ViewPager) getView();
        if (view == null)
            return null;
        return getProvider(view);
    }

    @Override
    public void setItemsSourceProvider(IItemsSourceProviderBase provider) {
        ViewPager view = (ViewPager) getView();
        if (view != null)
            setItemsSourceProvider(view, (IContentItemsSourceProvider) provider);
    }

    @Override
    public int getSelectedIndex() {
        ViewPager view = (ViewPager) getView();
        if (view == null)
            return 0;
        return view.getCurrentItem();
    }

    @Override
    public void setSelectedIndex(int index) {
        setSelectedIndex(index, true);
    }

    @Override
    public void setSelectedIndex(int index, boolean smoothScroll) {
        ViewPager view = (ViewPager) getView();
        if (view != null)
            view.setCurrentItem(index, smoothScroll);
    }

    @Override
    protected void addMemberListener(View view, String memberName) {
        super.addMemberListener(view, memberName);
        if (SelectedIndexName.equals(memberName) || SelectedIndexEventName.equals(memberName) && _selectedIndexChangedCount++ == 0) {
            if (_listener == null)
                _listener = new Listener();
            ((ViewPager) view).addOnPageChangeListener(_listener);
        }
    }

    @Override
    protected void removeMemberListener(View view, String memberName) {
        super.removeMemberListener(view, memberName);
        if (SelectedIndexName.equals(memberName) || SelectedIndexEventName.equals(memberName) && _selectedIndexChangedCount != 0 && --_selectedIndexChangedCount == 0)
            ((ViewPager) view).removeOnPageChangeListener(_listener);
    }


    @Override
    protected void onReleased(View target) {
        super.onReleased(target);
        setItemsSourceProvider((ViewPager) target, null);
    }

    private IContentItemsSourceProvider getProvider(ViewPager view) {
        PagerAdapter adapter = view.getAdapter();
        if (adapter instanceof MugenPagerAdapter) {
            IContentItemsSourceProvider provider = ((MugenPagerAdapter) adapter).getItemsSourceProvider();
            if (provider != null)
                return ((NativeContentItemsSourceProviderWrapper) provider).getNestedProvider();
        }
        return null;
    }

    private void setItemsSourceProvider(ViewPager view, IContentItemsSourceProvider provider) {
        if (getProvider(view) == provider)
            return;
        PagerAdapter adapter = view.getAdapter();
        if (provider == null) {
            if (adapter instanceof MugenPagerAdapter)
                ((MugenPagerAdapter) adapter).detach();
            view.setAdapter(null);
        } else
            view.setAdapter(new MugenPagerAdapter(view, new NativeContentItemsSourceProviderWrapper(provider)));
    }

    private final class Listener implements ViewPager.OnPageChangeListener {
        @Override
        public void onPageScrolled(int position, float positionOffset, int positionOffsetPixels) {
        }

        @Override
        public void onPageSelected(int position) {
            onMemberChanged(SelectedIndexName, null);
            onMemberChanged(SelectedIndexEventName, null);
        }

        @Override
        public void onPageScrollStateChanged(int state) {
        }
    }
}
