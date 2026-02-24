.class public final Lcom/kamcord/android/KC_q;
.super Ljava/util/ArrayList;

# interfaces
.implements Landroid/animation/ValueAnimator$AnimatorUpdateListener;
.implements Lcom/kamcord/android/KC_X;


# annotations
.annotation system Ldalvik/annotation/Signature;
    value = {
        "Ljava/util/ArrayList",
        "<",
        "La/a/a/c/KC_a;",
        ">;",
        "Landroid/animation/ValueAnimator$AnimatorUpdateListener;",
        "Lcom/kamcord/android/KC_X;"
    }
.end annotation


# instance fields
.field private a:Ljava/lang/ref/WeakReference;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/lang/ref/WeakReference",
            "<",
            "Lcom/kamcord/android/KamcordActivity;",
            ">;"
        }
    .end annotation
.end field

.field private b:Landroid/widget/RelativeLayout;

.field private c:Ljava/util/HashSet;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/HashSet",
            "<",
            "Lcom/kamcord/android/KC_r;",
            ">;"
        }
    .end annotation
.end field

.field private d:I

.field private e:Landroid/animation/ValueAnimator;


# direct methods
.method public constructor <init>(Lcom/kamcord/android/KamcordActivity;II)V
    .locals 8

    const/4 v7, 0x2

    const/16 v6, 0xb

    const/4 v5, 0x0

    const/4 v4, 0x1

    invoke-direct {p0}, Ljava/util/ArrayList;-><init>()V

    new-instance v0, Ljava/util/HashSet;

    invoke-direct {v0}, Ljava/util/HashSet;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/KC_q;->c:Ljava/util/HashSet;

    new-instance v0, Ljava/lang/ref/WeakReference;

    invoke-direct {v0, p1}, Ljava/lang/ref/WeakReference;-><init>(Ljava/lang/Object;)V

    iput-object v0, p0, Lcom/kamcord/android/KC_q;->a:Ljava/lang/ref/WeakReference;

    const-string v0, "id"

    const-string v1, "tabbar"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {p1, v0}, Lcom/kamcord/android/KamcordActivity;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/RelativeLayout;

    iput-object v0, p0, Lcom/kamcord/android/KC_q;->b:Landroid/widget/RelativeLayout;

    iget-object v0, p0, Lcom/kamcord/android/KC_q;->b:Landroid/widget/RelativeLayout;

    const-string v1, "id"

    const-string v2, "tabbar_items"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/RelativeLayout;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/LinearLayout;

    if-nez p2, :cond_1

    new-instance v1, Lcom/kamcord/android/KC_s;

    invoke-direct {v1}, Lcom/kamcord/android/KC_s;-><init>()V

    invoke-virtual {v1, p1}, Lcom/kamcord/android/KC_s;->a(Landroid/app/Activity;)Lcom/kamcord/android/KC_s;

    move-result-object v1

    sget-object v2, Lcom/kamcord/android/KC_p;->a:Lcom/kamcord/android/KC_p;

    invoke-virtual {v1, v2}, Lcom/kamcord/android/KC_s;->a(Lcom/kamcord/android/KC_p;)Lcom/kamcord/android/KC_s;

    move-result-object v1

    const-string v2, "drawable"

    const-string v3, "kamcord_share_icon"

    invoke-static {v2, v3}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v1, v2}, Lcom/kamcord/android/KC_s;->b(I)Lcom/kamcord/android/KC_s;

    move-result-object v1

    invoke-virtual {v1, v6}, Lcom/kamcord/android/KC_s;->c(I)Lcom/kamcord/android/KC_s;

    move-result-object v1

    invoke-virtual {v1, v5}, Lcom/kamcord/android/KC_s;->a(Z)Lcom/kamcord/android/KC_s;

    move-result-object v1

    invoke-virtual {v1, v5}, Lcom/kamcord/android/KC_s;->a(I)Lcom/kamcord/android/KC_s;

    move-result-object v1

    invoke-virtual {v1}, Lcom/kamcord/android/KC_s;->a()La/a/a/c/KC_a;

    move-result-object v1

    invoke-virtual {p0, v5, v1}, Lcom/kamcord/android/KC_q;->add(ILjava/lang/Object;)V

    new-instance v1, Lcom/kamcord/android/KC_s;

    invoke-direct {v1}, Lcom/kamcord/android/KC_s;-><init>()V

    invoke-virtual {v1, p1}, Lcom/kamcord/android/KC_s;->a(Landroid/app/Activity;)Lcom/kamcord/android/KC_s;

    move-result-object v1

    sget-object v2, Lcom/kamcord/android/KC_p;->b:Lcom/kamcord/android/KC_p;

    invoke-virtual {v1, v2}, Lcom/kamcord/android/KC_s;->a(Lcom/kamcord/android/KC_p;)Lcom/kamcord/android/KC_s;

    move-result-object v1

    const-string v2, "drawable"

    const-string v3, "kamcord_watch_icon"

    invoke-static {v2, v3}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v1, v2}, Lcom/kamcord/android/KC_s;->b(I)Lcom/kamcord/android/KC_s;

    move-result-object v1

    invoke-virtual {v1, v6}, Lcom/kamcord/android/KC_s;->c(I)Lcom/kamcord/android/KC_s;

    move-result-object v1

    invoke-virtual {v1, v4}, Lcom/kamcord/android/KC_s;->a(Z)Lcom/kamcord/android/KC_s;

    move-result-object v1

    invoke-virtual {v1, v4}, Lcom/kamcord/android/KC_s;->a(I)Lcom/kamcord/android/KC_s;

    move-result-object v1

    invoke-virtual {v1}, Lcom/kamcord/android/KC_s;->a()La/a/a/c/KC_a;

    move-result-object v1

    invoke-virtual {p0, v4, v1}, Lcom/kamcord/android/KC_q;->add(ILjava/lang/Object;)V

    new-instance v1, Lcom/kamcord/android/KC_s;

    invoke-direct {v1}, Lcom/kamcord/android/KC_s;-><init>()V

    invoke-virtual {v1, p1}, Lcom/kamcord/android/KC_s;->a(Landroid/app/Activity;)Lcom/kamcord/android/KC_s;

    move-result-object v1

    sget-object v2, Lcom/kamcord/android/KC_p;->c:Lcom/kamcord/android/KC_p;

    invoke-virtual {v1, v2}, Lcom/kamcord/android/KC_s;->a(Lcom/kamcord/android/KC_p;)Lcom/kamcord/android/KC_s;

    move-result-object v1

    const-string v2, "drawable"

    const-string v3, "kamcord_profile_icon"

    invoke-static {v2, v3}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v1, v2}, Lcom/kamcord/android/KC_s;->b(I)Lcom/kamcord/android/KC_s;

    move-result-object v1

    invoke-virtual {v1, v6}, Lcom/kamcord/android/KC_s;->c(I)Lcom/kamcord/android/KC_s;

    move-result-object v1

    invoke-virtual {v1, v4}, Lcom/kamcord/android/KC_s;->a(Z)Lcom/kamcord/android/KC_s;

    move-result-object v1

    invoke-virtual {v1, v7}, Lcom/kamcord/android/KC_s;->a(I)Lcom/kamcord/android/KC_s;

    move-result-object v1

    invoke-virtual {v1}, Lcom/kamcord/android/KC_s;->a()La/a/a/c/KC_a;

    move-result-object v1

    invoke-virtual {p0, v7, v1}, Lcom/kamcord/android/KC_q;->add(ILjava/lang/Object;)V

    :cond_0
    :goto_0
    invoke-virtual {p0}, Lcom/kamcord/android/KC_q;->iterator()Ljava/util/Iterator;

    move-result-object v2

    :goto_1
    invoke-interface {v2}, Ljava/util/Iterator;->hasNext()Z

    move-result v1

    if-eqz v1, :cond_2

    invoke-interface {v2}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v1

    check-cast v1, La/a/a/c/KC_a;

    invoke-virtual {v1}, La/a/a/c/KC_a;->f()Landroid/view/View;

    move-result-object v3

    new-instance v4, Lcom/kamcord/android/KC_q$1;

    invoke-direct {v4, p0, v1}, Lcom/kamcord/android/KC_q$1;-><init>(Lcom/kamcord/android/KC_q;La/a/a/c/KC_a;)V

    invoke-virtual {v3, v4}, Landroid/view/View;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    invoke-virtual {v1}, La/a/a/c/KC_a;->f()Landroid/view/View;

    move-result-object v3

    invoke-virtual {v0, v3}, Landroid/widget/LinearLayout;->addView(Landroid/view/View;)V

    invoke-virtual {v1}, La/a/a/c/KC_a;->e()I

    move-result v1

    invoke-direct {p0, v1}, Lcom/kamcord/android/KC_q;->b(I)V

    goto :goto_1

    :cond_1
    if-ne p2, v4, :cond_0

    new-instance v1, Lcom/kamcord/android/KC_s;

    invoke-direct {v1}, Lcom/kamcord/android/KC_s;-><init>()V

    invoke-virtual {v1, p1}, Lcom/kamcord/android/KC_s;->a(Landroid/app/Activity;)Lcom/kamcord/android/KC_s;

    move-result-object v1

    sget-object v2, Lcom/kamcord/android/KC_p;->b:Lcom/kamcord/android/KC_p;

    invoke-virtual {v1, v2}, Lcom/kamcord/android/KC_s;->a(Lcom/kamcord/android/KC_p;)Lcom/kamcord/android/KC_s;

    move-result-object v1

    const-string v2, "drawable"

    const-string v3, "kamcord_watch_icon"

    invoke-static {v2, v3}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v1, v2}, Lcom/kamcord/android/KC_s;->b(I)Lcom/kamcord/android/KC_s;

    move-result-object v1

    invoke-virtual {v1, v6}, Lcom/kamcord/android/KC_s;->c(I)Lcom/kamcord/android/KC_s;

    move-result-object v1

    invoke-virtual {v1, v4}, Lcom/kamcord/android/KC_s;->a(Z)Lcom/kamcord/android/KC_s;

    move-result-object v1

    invoke-virtual {v1, v5}, Lcom/kamcord/android/KC_s;->a(I)Lcom/kamcord/android/KC_s;

    move-result-object v1

    invoke-virtual {v1}, Lcom/kamcord/android/KC_s;->a()La/a/a/c/KC_a;

    move-result-object v1

    invoke-virtual {p0, v5, v1}, Lcom/kamcord/android/KC_q;->add(ILjava/lang/Object;)V

    new-instance v1, Lcom/kamcord/android/KC_s;

    invoke-direct {v1}, Lcom/kamcord/android/KC_s;-><init>()V

    invoke-virtual {v1, p1}, Lcom/kamcord/android/KC_s;->a(Landroid/app/Activity;)Lcom/kamcord/android/KC_s;

    move-result-object v1

    sget-object v2, Lcom/kamcord/android/KC_p;->c:Lcom/kamcord/android/KC_p;

    invoke-virtual {v1, v2}, Lcom/kamcord/android/KC_s;->a(Lcom/kamcord/android/KC_p;)Lcom/kamcord/android/KC_s;

    move-result-object v1

    const-string v2, "drawable"

    const-string v3, "kamcord_profile_icon"

    invoke-static {v2, v3}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v1, v2}, Lcom/kamcord/android/KC_s;->b(I)Lcom/kamcord/android/KC_s;

    move-result-object v1

    invoke-virtual {v1, v6}, Lcom/kamcord/android/KC_s;->c(I)Lcom/kamcord/android/KC_s;

    move-result-object v1

    invoke-virtual {v1, v4}, Lcom/kamcord/android/KC_s;->a(Z)Lcom/kamcord/android/KC_s;

    move-result-object v1

    invoke-virtual {v1, v4}, Lcom/kamcord/android/KC_s;->a(I)Lcom/kamcord/android/KC_s;

    move-result-object v1

    invoke-virtual {v1}, Lcom/kamcord/android/KC_s;->a()La/a/a/c/KC_a;

    move-result-object v1

    invoke-virtual {p0, v4, v1}, Lcom/kamcord/android/KC_q;->add(ILjava/lang/Object;)V

    goto/16 :goto_0

    :cond_2
    iput p3, p0, Lcom/kamcord/android/KC_q;->d:I

    invoke-direct {p0, p3}, Lcom/kamcord/android/KC_q;->a(I)V

    invoke-static {p0}, Lcom/kamcord/android/KC_Y;->a(Lcom/kamcord/android/KC_X;)V

    return-void
.end method

.method static synthetic a(Lcom/kamcord/android/KC_q;)Landroid/widget/RelativeLayout;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_q;->b:Landroid/widget/RelativeLayout;

    return-object v0
.end method

.method private a(F)V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_q;->a:Ljava/lang/ref/WeakReference;

    invoke-virtual {v0}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordActivity;

    new-instance v1, Lcom/kamcord/android/KC_q$2;

    invoke-direct {v1, p0, p1}, Lcom/kamcord/android/KC_q$2;-><init>(Lcom/kamcord/android/KC_q;F)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KamcordActivity;->runOnUiThread(Ljava/lang/Runnable;)V

    return-void
.end method

.method private a(I)V
    .locals 2

    invoke-virtual {p0, p1}, Lcom/kamcord/android/KC_q;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/c/KC_a;

    invoke-virtual {v0}, La/a/a/c/KC_a;->f()Landroid/view/View;

    move-result-object v0

    const/4 v1, 0x1

    invoke-virtual {v0, v1}, Landroid/view/View;->setSelected(Z)V

    return-void
.end method

.method static synthetic a(Lcom/kamcord/android/KC_q;F)V
    .locals 8

    const/4 v7, 0x2

    const/4 v6, 0x0

    const/4 v5, 0x1

    new-array v1, v7, [I

    iget-object v0, p0, Lcom/kamcord/android/KC_q;->b:Landroid/widget/RelativeLayout;

    const-string v2, "id"

    const-string v3, "tabbar_progress"

    invoke-static {v2, v3}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v0, v2}, Landroid/widget/RelativeLayout;->findViewById(I)Landroid/view/View;

    move-result-object v0

    invoke-virtual {v0}, Landroid/view/View;->getLayoutParams()Landroid/view/ViewGroup$LayoutParams;

    move-result-object v2

    iget-object v0, p0, Lcom/kamcord/android/KC_q;->a:Ljava/lang/ref/WeakReference;

    invoke-virtual {v0}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v0}, Lcom/kamcord/android/KamcordActivity;->getRequestedOrientation()I

    move-result v3

    if-eq v3, v5, :cond_0

    invoke-virtual {v0}, Lcom/kamcord/android/KamcordActivity;->getRequestedOrientation()I

    move-result v3

    const/16 v4, 0x9

    if-eq v3, v4, :cond_0

    invoke-virtual {v0}, Lcom/kamcord/android/KamcordActivity;->getResources()Landroid/content/res/Resources;

    move-result-object v3

    invoke-virtual {v3}, Landroid/content/res/Resources;->getConfiguration()Landroid/content/res/Configuration;

    move-result-object v3

    iget v3, v3, Landroid/content/res/Configuration;->smallestScreenWidthDp:I

    const/16 v4, 0x258

    if-lt v3, v4, :cond_3

    :cond_0
    iget v0, v2, Landroid/view/ViewGroup$LayoutParams;->width:I

    aput v0, v1, v6

    iget-object v0, p0, Lcom/kamcord/android/KC_q;->b:Landroid/widget/RelativeLayout;

    invoke-virtual {v0}, Landroid/widget/RelativeLayout;->getWidth()I

    move-result v0

    int-to-float v0, v0

    mul-float/2addr v0, p1

    float-to-int v0, v0

    aput v0, v1, v5

    :cond_1
    :goto_0
    iget-object v0, p0, Lcom/kamcord/android/KC_q;->e:Landroid/animation/ValueAnimator;

    if-eqz v0, :cond_2

    iget-object v0, p0, Lcom/kamcord/android/KC_q;->e:Landroid/animation/ValueAnimator;

    invoke-virtual {v0}, Landroid/animation/ValueAnimator;->cancel()V

    :cond_2
    new-array v0, v7, [I

    aget v2, v1, v6

    aput v2, v0, v6

    aget v1, v1, v5

    aput v1, v0, v5

    invoke-static {v0}, Landroid/animation/ValueAnimator;->ofInt([I)Landroid/animation/ValueAnimator;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_q;->e:Landroid/animation/ValueAnimator;

    iget-object v0, p0, Lcom/kamcord/android/KC_q;->e:Landroid/animation/ValueAnimator;

    const-wide/16 v2, 0x3e8

    invoke-virtual {v0, v2, v3}, Landroid/animation/ValueAnimator;->setDuration(J)Landroid/animation/ValueAnimator;

    iget-object v0, p0, Lcom/kamcord/android/KC_q;->e:Landroid/animation/ValueAnimator;

    new-instance v1, Landroid/view/animation/LinearInterpolator;

    invoke-direct {v1}, Landroid/view/animation/LinearInterpolator;-><init>()V

    invoke-virtual {v0, v1}, Landroid/animation/ValueAnimator;->setInterpolator(Landroid/animation/TimeInterpolator;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_q;->e:Landroid/animation/ValueAnimator;

    invoke-virtual {v0, p0}, Landroid/animation/ValueAnimator;->addUpdateListener(Landroid/animation/ValueAnimator$AnimatorUpdateListener;)V

    return-void

    :cond_3
    invoke-virtual {v0}, Lcom/kamcord/android/KamcordActivity;->getRequestedOrientation()I

    move-result v3

    if-eqz v3, :cond_4

    invoke-virtual {v0}, Lcom/kamcord/android/KamcordActivity;->getRequestedOrientation()I

    move-result v0

    const/16 v3, 0x8

    if-ne v0, v3, :cond_1

    :cond_4
    iget v0, v2, Landroid/view/ViewGroup$LayoutParams;->height:I

    aput v0, v1, v6

    iget-object v0, p0, Lcom/kamcord/android/KC_q;->b:Landroid/widget/RelativeLayout;

    invoke-virtual {v0}, Landroid/widget/RelativeLayout;->getHeight()I

    move-result v0

    int-to-float v0, v0

    mul-float/2addr v0, p1

    float-to-int v0, v0

    aput v0, v1, v5

    goto :goto_0
.end method

.method static synthetic b(Lcom/kamcord/android/KC_q;)Landroid/animation/ValueAnimator;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_q;->e:Landroid/animation/ValueAnimator;

    return-object v0
.end method

.method private b(I)V
    .locals 2

    invoke-virtual {p0, p1}, Lcom/kamcord/android/KC_q;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/c/KC_a;

    invoke-virtual {v0}, La/a/a/c/KC_a;->f()Landroid/view/View;

    move-result-object v0

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Landroid/view/View;->setSelected(Z)V

    return-void
.end method

.method static synthetic b(Lcom/kamcord/android/KC_q;F)V
    .locals 3

    const/high16 v0, 0x3f800000    # 1.0f

    const/4 v1, 0x0

    cmpg-float v2, v1, v1

    if-gez v2, :cond_0

    :cond_0
    cmpl-float v2, v1, v0

    if-lez v2, :cond_1

    move v1, v0

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_q;->a:Ljava/lang/ref/WeakReference;

    invoke-virtual {v0}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordActivity;

    new-instance v2, Lcom/kamcord/android/KC_q$4;

    invoke-direct {v2, p0, v0, v1}, Lcom/kamcord/android/KC_q$4;-><init>(Lcom/kamcord/android/KC_q;Lcom/kamcord/android/KamcordActivity;F)V

    invoke-virtual {v0, v2}, Lcom/kamcord/android/KamcordActivity;->runOnUiThread(Ljava/lang/Runnable;)V

    return-void
.end method

.method static synthetic c(Lcom/kamcord/android/KC_q;)Ljava/lang/ref/WeakReference;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_q;->a:Ljava/lang/ref/WeakReference;

    return-object v0
.end method

.method private declared-synchronized c(I)V
    .locals 3

    monitor-enter p0

    if-ltz p1, :cond_0

    :try_start_0
    invoke-virtual {p0}, Lcom/kamcord/android/KC_q;->size()I
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    move-result v0

    if-lt p1, v0, :cond_1

    :cond_0
    :goto_0
    monitor-exit p0

    return-void

    :cond_1
    :try_start_1
    iget-object v0, p0, Lcom/kamcord/android/KC_q;->c:Ljava/util/HashSet;

    invoke-virtual {v0}, Ljava/util/HashSet;->iterator()Ljava/util/Iterator;

    move-result-object v2

    :goto_1
    invoke-interface {v2}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_3

    invoke-interface {v2}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_r;

    iget v1, p0, Lcom/kamcord/android/KC_q;->d:I

    if-eq p1, v1, :cond_2

    invoke-virtual {p0, p1}, Lcom/kamcord/android/KC_q;->get(I)Ljava/lang/Object;

    move-result-object v1

    check-cast v1, La/a/a/c/KC_a;

    invoke-interface {v0, v1}, Lcom/kamcord/android/KC_r;->onTabSelected$7f50f85c(La/a/a/c/KC_a;)V

    iget v1, p0, Lcom/kamcord/android/KC_q;->d:I

    invoke-virtual {p0, v1}, Lcom/kamcord/android/KC_q;->get(I)Ljava/lang/Object;

    move-result-object v1

    check-cast v1, La/a/a/c/KC_a;

    invoke-interface {v0, v1}, Lcom/kamcord/android/KC_r;->onTabUnselected$7f50f85c(La/a/a/c/KC_a;)V

    iget v0, p0, Lcom/kamcord/android/KC_q;->d:I

    invoke-direct {p0, v0}, Lcom/kamcord/android/KC_q;->b(I)V
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    goto :goto_1

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0

    :cond_2
    :try_start_2
    iget v1, p0, Lcom/kamcord/android/KC_q;->d:I

    invoke-virtual {p0, v1}, Lcom/kamcord/android/KC_q;->get(I)Ljava/lang/Object;

    move-result-object v1

    check-cast v1, La/a/a/c/KC_a;

    invoke-interface {v0, v1}, Lcom/kamcord/android/KC_r;->onTabReselected$7f50f85c(La/a/a/c/KC_a;)V

    goto :goto_1

    :cond_3
    iput p1, p0, Lcom/kamcord/android/KC_q;->d:I
    :try_end_2
    .catchall {:try_start_2 .. :try_end_2} :catchall_0

    goto :goto_0
.end method

.method static synthetic d(Lcom/kamcord/android/KC_q;)V
    .locals 4

    iget-object v0, p0, Lcom/kamcord/android/KC_q;->b:Landroid/widget/RelativeLayout;

    const-string v1, "id"

    const-string v2, "tabbar_progress"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/RelativeLayout;->findViewById(I)Landroid/view/View;

    move-result-object v0

    new-instance v1, Landroid/view/animation/AlphaAnimation;

    const/high16 v2, 0x3f800000    # 1.0f

    const/4 v3, 0x0

    invoke-direct {v1, v2, v3}, Landroid/view/animation/AlphaAnimation;-><init>(FF)V

    const-wide/16 v2, 0x3e8

    invoke-virtual {v1, v2, v3}, Landroid/view/animation/AlphaAnimation;->setDuration(J)V

    new-instance v2, Landroid/view/animation/DecelerateInterpolator;

    invoke-direct {v2}, Landroid/view/animation/DecelerateInterpolator;-><init>()V

    invoke-virtual {v1, v2}, Landroid/view/animation/AlphaAnimation;->setInterpolator(Landroid/view/animation/Interpolator;)V

    new-instance v2, Lcom/kamcord/android/KC_q$3;

    invoke-direct {v2, p0}, Lcom/kamcord/android/KC_q$3;-><init>(Lcom/kamcord/android/KC_q;)V

    invoke-virtual {v1, v2}, Landroid/view/animation/AlphaAnimation;->setAnimationListener(Landroid/view/animation/Animation$AnimationListener;)V

    invoke-virtual {v0}, Landroid/view/View;->clearAnimation()V

    invoke-virtual {v0, v1}, Landroid/view/View;->startAnimation(Landroid/view/animation/Animation;)V

    return-void
.end method


# virtual methods
.method public final a()V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_q;->b:Landroid/widget/RelativeLayout;

    const/16 v1, 0x8

    invoke-virtual {v0, v1}, Landroid/widget/RelativeLayout;->setVisibility(I)V

    return-void
.end method

.method protected final a(La/a/a/c/KC_a;)V
    .locals 1

    invoke-virtual {p1}, La/a/a/c/KC_a;->e()I

    move-result v0

    invoke-direct {p0, v0}, Lcom/kamcord/android/KC_q;->a(I)V

    invoke-direct {p0, v0}, Lcom/kamcord/android/KC_q;->c(I)V

    return-void
.end method

.method public final a(Lcom/kamcord/android/KC_r;)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_q;->c:Ljava/util/HashSet;

    invoke-virtual {v0, p1}, Ljava/util/HashSet;->add(Ljava/lang/Object;)Z

    return-void
.end method

.method public final a(Ljava/lang/String;F)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->b()Lcom/kamcord/android/core/KC_H;

    move-result-object v0

    if-eqz v0, :cond_0

    iget-object v0, v0, Lcom/kamcord/android/core/KC_H;->a:Ljava/lang/String;

    invoke-virtual {v0, p1}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-nez v0, :cond_1

    :cond_0
    :goto_0
    return-void

    :cond_1
    invoke-direct {p0, p2}, Lcom/kamcord/android/KC_q;->a(F)V

    goto :goto_0
.end method

.method public final a(Ljava/lang/String;I)V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->b()Lcom/kamcord/android/core/KC_H;

    move-result-object v0

    if-eqz v0, :cond_0

    iget-object v0, v0, Lcom/kamcord/android/core/KC_H;->a:Ljava/lang/String;

    invoke-virtual {v0, p1}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-nez v0, :cond_1

    :cond_0
    :goto_0
    return-void

    :cond_1
    const/4 v0, 0x0

    invoke-direct {p0, v0}, Lcom/kamcord/android/KC_q;->a(F)V

    iget-object v0, p0, Lcom/kamcord/android/KC_q;->a:Ljava/lang/ref/WeakReference;

    invoke-virtual {v0}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordActivity;

    new-instance v1, Lcom/kamcord/android/KC_q$5;

    invoke-direct {v1, p0, p2}, Lcom/kamcord/android/KC_q$5;-><init>(Lcom/kamcord/android/KC_q;I)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KamcordActivity;->runOnUiThread(Ljava/lang/Runnable;)V

    goto :goto_0
.end method

.method public final a(Ljava/lang/String;Ljava/lang/String;)V
    .locals 0

    return-void
.end method

.method public final a(Ljava/util/HashSet;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V
    .locals 0
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Ljava/util/HashSet",
            "<",
            "Ljava/lang/String;",
            ">;",
            "Ljava/lang/String;",
            "Ljava/lang/String;",
            "Ljava/lang/String;",
            ")V"
        }
    .end annotation

    return-void
.end method

.method public final a_(Ljava/lang/String;)V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->b()Lcom/kamcord/android/core/KC_H;

    move-result-object v0

    if-eqz v0, :cond_0

    iget-object v0, v0, Lcom/kamcord/android/core/KC_H;->a:Ljava/lang/String;

    invoke-virtual {v0, p1}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-nez v0, :cond_1

    :cond_0
    :goto_0
    return-void

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_q;->a:Ljava/lang/ref/WeakReference;

    invoke-virtual {v0}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordActivity;

    new-instance v1, Lcom/kamcord/android/KC_q$6;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_q$6;-><init>(Lcom/kamcord/android/KC_q;)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KamcordActivity;->runOnUiThread(Ljava/lang/Runnable;)V

    goto :goto_0
.end method

.method public final a_(Ljava/lang/String;Z)V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_q;->a:Ljava/lang/ref/WeakReference;

    invoke-virtual {v0}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordActivity;

    new-instance v1, Lcom/kamcord/android/KC_q$7;

    invoke-direct {v1, p0, p2}, Lcom/kamcord/android/KC_q$7;-><init>(Lcom/kamcord/android/KC_q;Z)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KamcordActivity;->runOnUiThread(Ljava/lang/Runnable;)V

    return-void
.end method

.method public final b()V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_q;->b:Landroid/widget/RelativeLayout;

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Landroid/widget/RelativeLayout;->setVisibility(I)V

    return-void
.end method

.method public final b(Ljava/lang/String;)V
    .locals 0

    return-void
.end method

.method public final c()La/a/a/c/KC_a;
    .locals 1

    iget v0, p0, Lcom/kamcord/android/KC_q;->d:I

    invoke-virtual {p0, v0}, Lcom/kamcord/android/KC_q;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/c/KC_a;

    return-object v0
.end method

.method public final onAnimationUpdate(Landroid/animation/ValueAnimator;)V
    .locals 3

    invoke-virtual {p1}, Landroid/animation/ValueAnimator;->getAnimatedValue()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/lang/Integer;

    invoke-virtual {v0}, Ljava/lang/Integer;->intValue()I

    move-result v1

    iget-object v0, p0, Lcom/kamcord/android/KC_q;->a:Ljava/lang/ref/WeakReference;

    invoke-virtual {v0}, Ljava/lang/ref/WeakReference;->get()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordActivity;

    new-instance v2, Lcom/kamcord/android/KC_q$8;

    invoke-direct {v2, p0, v0, v1}, Lcom/kamcord/android/KC_q$8;-><init>(Lcom/kamcord/android/KC_q;Lcom/kamcord/android/KamcordActivity;I)V

    invoke-virtual {v0, v2}, Lcom/kamcord/android/KamcordActivity;->runOnUiThread(Ljava/lang/Runnable;)V

    return-void
.end method
