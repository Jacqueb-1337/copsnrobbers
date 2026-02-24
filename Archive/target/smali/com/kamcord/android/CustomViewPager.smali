.class public Lcom/kamcord/android/CustomViewPager;
.super La/a/a/e/KC_i;


# instance fields
.field private a:Z


# direct methods
.method public constructor <init>(Landroid/content/Context;Landroid/util/AttributeSet;)V
    .locals 1

    invoke-direct {p0, p1, p2}, La/a/a/e/KC_i;-><init>(Landroid/content/Context;Landroid/util/AttributeSet;)V

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/CustomViewPager;->a:Z

    return-void
.end method


# virtual methods
.method public onInterceptTouchEvent(Landroid/view/MotionEvent;)Z
    .locals 1

    iget-boolean v0, p0, Lcom/kamcord/android/CustomViewPager;->a:Z

    if-eqz v0, :cond_0

    invoke-super {p0, p1}, La/a/a/e/KC_i;->onInterceptTouchEvent(Landroid/view/MotionEvent;)Z

    move-result v0

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public onTouchEvent(Landroid/view/MotionEvent;)Z
    .locals 1

    iget-boolean v0, p0, Lcom/kamcord/android/CustomViewPager;->a:Z

    if-eqz v0, :cond_0

    invoke-super {p0, p1}, La/a/a/e/KC_i;->onTouchEvent(Landroid/view/MotionEvent;)Z

    move-result v0

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public setPagingEnabled(Z)V
    .locals 0

    iput-boolean p1, p0, Lcom/kamcord/android/CustomViewPager;->a:Z

    return-void
.end method
