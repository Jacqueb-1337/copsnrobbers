.class public Lcom/kamcord/android/CustomWebView;
.super Landroid/widget/RelativeLayout;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/CustomWebView$KC_a;
    }
.end annotation


# instance fields
.field private a:I

.field private b:Landroid/view/ViewGroup$LayoutParams;

.field private c:Landroid/webkit/WebView;

.field private d:Z

.field private e:Z

.field private f:F

.field private g:F

.field private h:Ljava/util/HashSet;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/HashSet",
            "<",
            "Lcom/kamcord/android/CustomWebView$KC_a;",
            ">;"
        }
    .end annotation
.end field


# direct methods
.method public constructor <init>(Landroid/content/Context;)V
    .locals 2

    const/4 v1, 0x0

    invoke-direct {p0, p1}, Landroid/widget/RelativeLayout;-><init>(Landroid/content/Context;)V

    const/4 v0, -0x1

    iput v0, p0, Lcom/kamcord/android/CustomWebView;->a:I

    const/4 v0, 0x0

    iput-object v0, p0, Lcom/kamcord/android/CustomWebView;->b:Landroid/view/ViewGroup$LayoutParams;

    iput-boolean v1, p0, Lcom/kamcord/android/CustomWebView;->d:Z

    iput-boolean v1, p0, Lcom/kamcord/android/CustomWebView;->e:Z

    const v0, 0x3e99999a    # 0.3f

    iput v0, p0, Lcom/kamcord/android/CustomWebView;->g:F

    new-instance v0, Ljava/util/HashSet;

    invoke-direct {v0}, Ljava/util/HashSet;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/CustomWebView;->h:Ljava/util/HashSet;

    new-instance v0, Landroid/webkit/WebView;

    invoke-direct {v0, p1}, Landroid/webkit/WebView;-><init>(Landroid/content/Context;)V

    iput-object v0, p0, Lcom/kamcord/android/CustomWebView;->c:Landroid/webkit/WebView;

    invoke-direct {p0}, Lcom/kamcord/android/CustomWebView;->b()V

    return-void
.end method

.method public constructor <init>(Landroid/content/Context;Landroid/util/AttributeSet;)V
    .locals 2

    const/4 v1, 0x0

    invoke-direct {p0, p1, p2}, Landroid/widget/RelativeLayout;-><init>(Landroid/content/Context;Landroid/util/AttributeSet;)V

    const/4 v0, -0x1

    iput v0, p0, Lcom/kamcord/android/CustomWebView;->a:I

    const/4 v0, 0x0

    iput-object v0, p0, Lcom/kamcord/android/CustomWebView;->b:Landroid/view/ViewGroup$LayoutParams;

    iput-boolean v1, p0, Lcom/kamcord/android/CustomWebView;->d:Z

    iput-boolean v1, p0, Lcom/kamcord/android/CustomWebView;->e:Z

    const v0, 0x3e99999a    # 0.3f

    iput v0, p0, Lcom/kamcord/android/CustomWebView;->g:F

    new-instance v0, Ljava/util/HashSet;

    invoke-direct {v0}, Ljava/util/HashSet;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/CustomWebView;->h:Ljava/util/HashSet;

    new-instance v0, Landroid/webkit/WebView;

    invoke-direct {v0, p1, p2}, Landroid/webkit/WebView;-><init>(Landroid/content/Context;Landroid/util/AttributeSet;)V

    iput-object v0, p0, Lcom/kamcord/android/CustomWebView;->c:Landroid/webkit/WebView;

    invoke-direct {p0}, Lcom/kamcord/android/CustomWebView;->b()V

    return-void
.end method

.method public constructor <init>(Landroid/content/Context;Landroid/util/AttributeSet;I)V
    .locals 2

    const/4 v1, 0x0

    invoke-direct {p0, p1, p2, p3}, Landroid/widget/RelativeLayout;-><init>(Landroid/content/Context;Landroid/util/AttributeSet;I)V

    const/4 v0, -0x1

    iput v0, p0, Lcom/kamcord/android/CustomWebView;->a:I

    const/4 v0, 0x0

    iput-object v0, p0, Lcom/kamcord/android/CustomWebView;->b:Landroid/view/ViewGroup$LayoutParams;

    iput-boolean v1, p0, Lcom/kamcord/android/CustomWebView;->d:Z

    iput-boolean v1, p0, Lcom/kamcord/android/CustomWebView;->e:Z

    const v0, 0x3e99999a    # 0.3f

    iput v0, p0, Lcom/kamcord/android/CustomWebView;->g:F

    new-instance v0, Ljava/util/HashSet;

    invoke-direct {v0}, Ljava/util/HashSet;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/CustomWebView;->h:Ljava/util/HashSet;

    new-instance v0, Landroid/webkit/WebView;

    invoke-direct {v0, p1, p2, p3}, Landroid/webkit/WebView;-><init>(Landroid/content/Context;Landroid/util/AttributeSet;I)V

    iput-object v0, p0, Lcom/kamcord/android/CustomWebView;->c:Landroid/webkit/WebView;

    invoke-direct {p0}, Lcom/kamcord/android/CustomWebView;->b()V

    return-void
.end method

.method static synthetic a(Lcom/kamcord/android/CustomWebView;)Landroid/webkit/WebView;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/CustomWebView;->c:Landroid/webkit/WebView;

    return-object v0
.end method

.method static synthetic a(Lcom/kamcord/android/CustomWebView;Landroid/view/MotionEvent;)V
    .locals 8

    const/4 v6, 0x0

    const/high16 v5, 0x3f800000    # 1.0f

    const/4 v2, 0x0

    const/16 v4, 0x64

    invoke-virtual {p1}, Landroid/view/MotionEvent;->getAction()I

    move-result v0

    packed-switch v0, :pswitch_data_0

    :cond_0
    :goto_0
    return-void

    :pswitch_0
    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/CustomWebView;->e:Z

    invoke-virtual {p1}, Landroid/view/MotionEvent;->getY()F

    move-result v0

    iput v0, p0, Lcom/kamcord/android/CustomWebView;->f:F

    invoke-virtual {p0, v4}, Lcom/kamcord/android/CustomWebView;->findViewById(I)Landroid/view/View;

    move-result-object v0

    if-nez v0, :cond_0

    invoke-virtual {p0}, Lcom/kamcord/android/CustomWebView;->getScrollY()I

    move-result v0

    if-nez v0, :cond_0

    new-instance v0, Landroid/widget/RelativeLayout$LayoutParams;

    invoke-virtual {p0}, Lcom/kamcord/android/CustomWebView;->getContext()Landroid/content/Context;

    move-result-object v1

    invoke-virtual {v1}, Landroid/content/Context;->getResources()Landroid/content/res/Resources;

    move-result-object v1

    invoke-virtual {v1}, Landroid/content/res/Resources;->getDisplayMetrics()Landroid/util/DisplayMetrics;

    move-result-object v1

    const/high16 v2, 0x40800000    # 4.0f

    iget v1, v1, Landroid/util/DisplayMetrics;->xdpi:F

    const/high16 v3, 0x43200000    # 160.0f

    div-float/2addr v1, v3

    mul-float/2addr v1, v2

    invoke-static {v1}, Ljava/lang/Math;->round(F)I

    move-result v1

    invoke-direct {v0, v6, v1}, Landroid/widget/RelativeLayout$LayoutParams;-><init>(II)V

    const/16 v1, 0xa

    invoke-virtual {v0, v1}, Landroid/widget/RelativeLayout$LayoutParams;->addRule(I)V

    const/16 v1, 0xe

    invoke-virtual {v0, v1}, Landroid/widget/RelativeLayout$LayoutParams;->addRule(I)V

    new-instance v1, Landroid/widget/LinearLayout;

    invoke-virtual {p0}, Lcom/kamcord/android/CustomWebView;->getContext()Landroid/content/Context;

    move-result-object v2

    invoke-direct {v1, v2}, Landroid/widget/LinearLayout;-><init>(Landroid/content/Context;)V

    invoke-virtual {v1, v0}, Landroid/widget/LinearLayout;->setLayoutParams(Landroid/view/ViewGroup$LayoutParams;)V

    invoke-virtual {v1, v4}, Landroid/widget/LinearLayout;->setId(I)V

    const-string v0, "color"

    const-string v2, "kamcordButtonActive"

    invoke-static {v0, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {v1, v0}, Landroid/widget/LinearLayout;->setBackgroundResource(I)V

    invoke-virtual {p0, v1}, Lcom/kamcord/android/CustomWebView;->addView(Landroid/view/View;)V

    goto :goto_0

    :pswitch_1
    iput-boolean v6, p0, Lcom/kamcord/android/CustomWebView;->e:Z

    invoke-virtual {p0, v4}, Lcom/kamcord/android/CustomWebView;->findViewById(I)Landroid/view/View;

    move-result-object v0

    if-eqz v0, :cond_0

    invoke-virtual {p0, v0}, Lcom/kamcord/android/CustomWebView;->removeView(Landroid/view/View;)V

    goto :goto_0

    :pswitch_2
    iget-boolean v0, p0, Lcom/kamcord/android/CustomWebView;->e:Z

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/CustomWebView;->c:Landroid/webkit/WebView;

    const/4 v1, -0x1

    invoke-virtual {v0, v1}, Landroid/webkit/WebView;->canScrollVertically(I)Z

    move-result v0

    if-nez v0, :cond_0

    invoke-virtual {p0, v4}, Lcom/kamcord/android/CustomWebView;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/LinearLayout;

    if-eqz v0, :cond_0

    invoke-virtual {p1}, Landroid/view/MotionEvent;->getY()F

    move-result v1

    iget v3, p0, Lcom/kamcord/android/CustomWebView;->f:F

    sub-float/2addr v1, v3

    cmpg-float v3, v1, v2

    if-gez v3, :cond_1

    iget v3, p0, Lcom/kamcord/android/CustomWebView;->f:F

    add-float/2addr v1, v3

    iput v1, p0, Lcom/kamcord/android/CustomWebView;->f:F

    move v1, v2

    :cond_1
    invoke-virtual {p0}, Lcom/kamcord/android/CustomWebView;->getHeight()I

    move-result v3

    int-to-float v3, v3

    iget v4, p0, Lcom/kamcord/android/CustomWebView;->g:F

    mul-float/2addr v3, v4

    div-float/2addr v1, v3

    invoke-static {v1, v2}, Ljava/lang/Math;->max(FF)F

    move-result v1

    invoke-static {v1, v5}, Ljava/lang/Math;->min(FF)F

    move-result v2

    cmpl-float v1, v2, v5

    if-nez v1, :cond_2

    const-string v1, "CustomWebView"

    const-string v3, "Reloading page."

    invoke-static {v1, v3}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;Ljava/lang/String;)I

    invoke-virtual {p0, v0}, Lcom/kamcord/android/CustomWebView;->removeView(Landroid/view/View;)V

    iget-object v1, p0, Lcom/kamcord/android/CustomWebView;->h:Ljava/util/HashSet;

    invoke-virtual {v1}, Ljava/util/HashSet;->iterator()Ljava/util/Iterator;

    move-result-object v3

    :goto_1
    invoke-interface {v3}, Ljava/util/Iterator;->hasNext()Z

    move-result v1

    if-eqz v1, :cond_2

    invoke-interface {v3}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v1

    check-cast v1, Lcom/kamcord/android/CustomWebView$KC_a;

    invoke-interface {v1}, Lcom/kamcord/android/CustomWebView$KC_a;->a_()V

    goto :goto_1

    :cond_2
    invoke-virtual {v0}, Landroid/widget/LinearLayout;->getLayoutParams()Landroid/view/ViewGroup$LayoutParams;

    move-result-object v1

    check-cast v1, Landroid/widget/RelativeLayout$LayoutParams;

    invoke-virtual {p0}, Lcom/kamcord/android/CustomWebView;->getWidth()I

    move-result v3

    int-to-double v4, v3

    float-to-double v2, v2

    const-wide/high16 v6, 0x3fe0000000000000L    # 0.5

    invoke-static {v2, v3, v6, v7}, Ljava/lang/Math;->pow(DD)D

    move-result-wide v2

    mul-double/2addr v2, v4

    double-to-int v2, v2

    iput v2, v1, Landroid/widget/RelativeLayout$LayoutParams;->width:I

    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->setLayoutParams(Landroid/view/ViewGroup$LayoutParams;)V

    goto/16 :goto_0

    :pswitch_data_0
    .packed-switch 0x0
        :pswitch_0
        :pswitch_1
        :pswitch_2
    .end packed-switch
.end method

.method private b()V
    .locals 3

    const/4 v2, -0x1

    iget-object v0, p0, Lcom/kamcord/android/CustomWebView;->c:Landroid/webkit/WebView;

    new-instance v1, Landroid/view/ViewGroup$LayoutParams;

    invoke-direct {v1, v2, v2}, Landroid/view/ViewGroup$LayoutParams;-><init>(II)V

    invoke-virtual {v0, v1}, Landroid/webkit/WebView;->setLayoutParams(Landroid/view/ViewGroup$LayoutParams;)V

    iget-object v0, p0, Lcom/kamcord/android/CustomWebView;->c:Landroid/webkit/WebView;

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Landroid/webkit/WebView;->setBackgroundColor(I)V

    iget-object v0, p0, Lcom/kamcord/android/CustomWebView;->c:Landroid/webkit/WebView;

    invoke-virtual {p0, v0}, Lcom/kamcord/android/CustomWebView;->addView(Landroid/view/View;)V

    iget-object v0, p0, Lcom/kamcord/android/CustomWebView;->c:Landroid/webkit/WebView;

    new-instance v1, Lcom/kamcord/android/CustomWebView$1;

    invoke-direct {v1, p0}, Lcom/kamcord/android/CustomWebView$1;-><init>(Lcom/kamcord/android/CustomWebView;)V

    invoke-virtual {v0, v1}, Landroid/webkit/WebView;->setOnTouchListener(Landroid/view/View$OnTouchListener;)V

    return-void
.end method

.method static synthetic b(Lcom/kamcord/android/CustomWebView;)Z
    .locals 1

    iget-boolean v0, p0, Lcom/kamcord/android/CustomWebView;->d:Z

    return v0
.end method

.method private c()I
    .locals 2

    new-instance v0, Landroid/graphics/Rect;

    invoke-direct {v0}, Landroid/graphics/Rect;-><init>()V

    invoke-virtual {p0, v0}, Lcom/kamcord/android/CustomWebView;->getWindowVisibleDisplayFrame(Landroid/graphics/Rect;)V

    iget v1, v0, Landroid/graphics/Rect;->bottom:I

    iget v0, v0, Landroid/graphics/Rect;->top:I

    sub-int v0, v1, v0

    return v0
.end method

.method static synthetic c(Lcom/kamcord/android/CustomWebView;)V
    .locals 5

    iget-object v0, p0, Lcom/kamcord/android/CustomWebView;->b:Landroid/view/ViewGroup$LayoutParams;

    if-nez v0, :cond_0

    invoke-virtual {p0}, Lcom/kamcord/android/CustomWebView;->getLayoutParams()Landroid/view/ViewGroup$LayoutParams;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/CustomWebView;->b:Landroid/view/ViewGroup$LayoutParams;

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/CustomWebView;->b:Landroid/view/ViewGroup$LayoutParams;

    if-eqz v0, :cond_1

    invoke-direct {p0}, Lcom/kamcord/android/CustomWebView;->c()I

    move-result v0

    iget v1, p0, Lcom/kamcord/android/CustomWebView;->a:I

    if-eq v0, v1, :cond_1

    invoke-virtual {p0}, Lcom/kamcord/android/CustomWebView;->getRootView()Landroid/view/View;

    move-result-object v1

    invoke-virtual {v1}, Landroid/view/View;->getHeight()I

    move-result v1

    invoke-virtual {p0}, Lcom/kamcord/android/CustomWebView;->getRootView()Landroid/view/View;

    move-result-object v2

    const-string v3, "id"

    const-string v4, "kamcord_main"

    invoke-static {v3, v4}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v3

    invoke-virtual {v2, v3}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v2

    if-eqz v2, :cond_1

    invoke-virtual {v2}, Landroid/view/View;->getBottom()I

    move-result v2

    sub-int v3, v1, v0

    if-lez v3, :cond_2

    neg-int v4, v3

    add-int/2addr v1, v4

    sub-int/2addr v1, v2

    int-to-float v1, v1

    invoke-virtual {p0, v1}, Lcom/kamcord/android/CustomWebView;->setTranslationY(F)V

    :goto_0
    const-string v1, "ClickCorrectWebView"

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v4, "translating by "

    invoke-direct {v2, v4}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;Ljava/lang/String;)I

    iput v0, p0, Lcom/kamcord/android/CustomWebView;->a:I

    :cond_1
    return-void

    :cond_2
    neg-int v1, v3

    int-to-float v1, v1

    invoke-virtual {p0, v1}, Lcom/kamcord/android/CustomWebView;->setTranslationY(F)V

    goto :goto_0
.end method


# virtual methods
.method final a()V
    .locals 2

    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v1, 0xb

    if-lt v0, v1, :cond_0

    invoke-direct {p0}, Lcom/kamcord/android/CustomWebView;->c()I

    move-result v0

    iput v0, p0, Lcom/kamcord/android/CustomWebView;->a:I

    invoke-virtual {p0}, Lcom/kamcord/android/CustomWebView;->getViewTreeObserver()Landroid/view/ViewTreeObserver;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/CustomWebView$2;

    invoke-direct {v1, p0}, Lcom/kamcord/android/CustomWebView$2;-><init>(Lcom/kamcord/android/CustomWebView;)V

    invoke-virtual {v0, v1}, Landroid/view/ViewTreeObserver;->addOnGlobalLayoutListener(Landroid/view/ViewTreeObserver$OnGlobalLayoutListener;)V

    :cond_0
    return-void
.end method

.method public addPullToRefreshListener(Lcom/kamcord/android/CustomWebView$KC_a;)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/CustomWebView;->h:Ljava/util/HashSet;

    invoke-virtual {v0, p1}, Ljava/util/HashSet;->add(Ljava/lang/Object;)Z

    return-void
.end method

.method public destroy()V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/CustomWebView;->c:Landroid/webkit/WebView;

    invoke-virtual {v0}, Landroid/webkit/WebView;->destroy()V

    const/4 v0, 0x0

    iput-object v0, p0, Lcom/kamcord/android/CustomWebView;->c:Landroid/webkit/WebView;

    return-void
.end method

.method public getSettings()Landroid/webkit/WebSettings;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/CustomWebView;->c:Landroid/webkit/WebView;

    invoke-virtual {v0}, Landroid/webkit/WebView;->getSettings()Landroid/webkit/WebSettings;

    move-result-object v0

    return-object v0
.end method

.method public getWebView()Landroid/webkit/WebView;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/CustomWebView;->c:Landroid/webkit/WebView;

    return-object v0
.end method

.method public loadData(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/CustomWebView;->c:Landroid/webkit/WebView;

    invoke-virtual {v0, p1, p2, p3}, Landroid/webkit/WebView;->loadData(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V

    return-void
.end method

.method public loadUrl(Ljava/lang/String;)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/CustomWebView;->c:Landroid/webkit/WebView;

    invoke-virtual {v0, p1}, Landroid/webkit/WebView;->loadUrl(Ljava/lang/String;)V

    return-void
.end method

.method public postUrl(Ljava/lang/String;[B)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/CustomWebView;->c:Landroid/webkit/WebView;

    invoke-virtual {v0, p1, p2}, Landroid/webkit/WebView;->postUrl(Ljava/lang/String;[B)V

    return-void
.end method

.method public removePullToRefreshListener(Lcom/kamcord/android/CustomWebView$KC_a;)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/CustomWebView;->h:Ljava/util/HashSet;

    invoke-virtual {v0, p1}, Ljava/util/HashSet;->remove(Ljava/lang/Object;)Z

    return-void
.end method

.method public setIsRefreshable(Z)V
    .locals 0

    iput-boolean p1, p0, Lcom/kamcord/android/CustomWebView;->d:Z

    return-void
.end method

.method public setRefreshDistanceHeightFraction(F)V
    .locals 0

    iput p1, p0, Lcom/kamcord/android/CustomWebView;->g:F

    return-void
.end method

.method public setWebViewClient(Landroid/webkit/WebViewClient;)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/CustomWebView;->c:Landroid/webkit/WebView;

    invoke-virtual {v0, p1}, Landroid/webkit/WebView;->setWebViewClient(Landroid/webkit/WebViewClient;)V

    return-void
.end method
