.class final Lcom/kamcord/android/KC_s;
.super Ljava/lang/Object;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/KC_s$KC_a;
    }
.end annotation


# instance fields
.field private a:Lcom/kamcord/android/KC_p;

.field private b:Ljava/lang/String;

.field private c:Z

.field private d:I

.field private e:Landroid/app/Activity;

.field private f:Lcom/kamcord/android/KC_s$KC_a;

.field private g:I

.field private h:F


# direct methods
.method constructor <init>()V
    .locals 3

    const/4 v2, 0x0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    sget-object v0, Lcom/kamcord/android/KC_p;->a:Lcom/kamcord/android/KC_p;

    iput-object v0, p0, Lcom/kamcord/android/KC_s;->a:Lcom/kamcord/android/KC_p;

    const-string v0, "kamcordShare"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    sget-object v1, Ljava/util/Locale;->ENGLISH:Ljava/util/Locale;

    invoke-virtual {v0, v1}, Ljava/lang/String;->toUpperCase(Ljava/util/Locale;)Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_s;->b:Ljava/lang/String;

    iput-boolean v2, p0, Lcom/kamcord/android/KC_s;->c:Z

    iput v2, p0, Lcom/kamcord/android/KC_s;->d:I

    const/4 v0, 0x0

    iput-object v0, p0, Lcom/kamcord/android/KC_s;->e:Landroid/app/Activity;

    sget-object v0, Lcom/kamcord/android/KC_s$KC_a;->a:Lcom/kamcord/android/KC_s$KC_a;

    iput-object v0, p0, Lcom/kamcord/android/KC_s;->f:Lcom/kamcord/android/KC_s$KC_a;

    iput v2, p0, Lcom/kamcord/android/KC_s;->g:I

    const/4 v0, 0x0

    iput v0, p0, Lcom/kamcord/android/KC_s;->h:F

    return-void
.end method


# virtual methods
.method public final a()La/a/a/c/KC_a;
    .locals 11

    const/4 v4, 0x0

    const/high16 v7, 0x3f800000    # 1.0f

    const/4 v2, 0x1

    const/4 v10, -0x1

    const/4 v1, 0x0

    new-instance v5, La/a/a/c/KC_a;

    invoke-direct {v5}, La/a/a/c/KC_a;-><init>()V

    iget-object v0, p0, Lcom/kamcord/android/KC_s;->e:Landroid/app/Activity;

    if-nez v0, :cond_0

    const-string v0, "You must provide a context to build a tab!"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    move-object v0, v4

    :goto_0
    return-object v0

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_s;->a:Lcom/kamcord/android/KC_p;

    invoke-virtual {v5, v0}, La/a/a/c/KC_a;->a(Lcom/kamcord/android/KC_p;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_s;->b:Ljava/lang/String;

    invoke-virtual {v5, v0}, La/a/a/c/KC_a;->a(Ljava/lang/String;)V

    iget-boolean v0, p0, Lcom/kamcord/android/KC_s;->c:Z

    invoke-virtual {v5, v0}, La/a/a/c/KC_a;->a(Z)V

    iget v0, p0, Lcom/kamcord/android/KC_s;->d:I

    invoke-virtual {v5, v0}, La/a/a/c/KC_a;->a(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_s;->e:Landroid/app/Activity;

    invoke-virtual {v0}, Landroid/app/Activity;->getRequestedOrientation()I

    move-result v0

    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/KC_s;->e:Landroid/app/Activity;

    invoke-virtual {v0}, Landroid/app/Activity;->getRequestedOrientation()I

    move-result v0

    const/16 v3, 0x8

    if-ne v0, v3, :cond_3

    :cond_1
    move v0, v2

    :goto_1
    iget-object v3, p0, Lcom/kamcord/android/KC_s;->e:Landroid/app/Activity;

    invoke-virtual {v3}, Landroid/app/Activity;->getResources()Landroid/content/res/Resources;

    move-result-object v3

    invoke-virtual {v3}, Landroid/content/res/Resources;->getConfiguration()Landroid/content/res/Configuration;

    move-result-object v3

    iget v3, v3, Landroid/content/res/Configuration;->smallestScreenWidthDp:I

    const/16 v6, 0x258

    if-lt v3, v6, :cond_4

    move v6, v2

    :goto_2
    if-eqz v0, :cond_5

    if-nez v6, :cond_5

    new-instance v3, Landroid/widget/LinearLayout$LayoutParams;

    invoke-direct {v3, v10, v1, v7}, Landroid/widget/LinearLayout$LayoutParams;-><init>(IIF)V

    iget-object v7, p0, Lcom/kamcord/android/KC_s;->e:Landroid/app/Activity;

    invoke-virtual {v7}, Landroid/app/Activity;->getResources()Landroid/content/res/Resources;

    move-result-object v7

    const-string v8, "dimen"

    const-string v9, "kamcordTabbarItemSeparation"

    invoke-static {v8, v9}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v8

    invoke-virtual {v7, v8}, Landroid/content/res/Resources;->getDimensionPixelSize(I)I

    move-result v7

    iput v7, v3, Landroid/widget/LinearLayout$LayoutParams;->topMargin:I

    :goto_3
    iget-object v7, p0, Lcom/kamcord/android/KC_s;->f:Lcom/kamcord/android/KC_s$KC_a;

    sget-object v8, Lcom/kamcord/android/KC_s$KC_a;->b:Lcom/kamcord/android/KC_s$KC_a;

    if-ne v7, v8, :cond_7

    new-instance v2, Landroid/widget/ImageView;

    iget-object v4, p0, Lcom/kamcord/android/KC_s;->e:Landroid/app/Activity;

    invoke-direct {v2, v4}, Landroid/widget/ImageView;-><init>(Landroid/content/Context;)V

    iget v4, p0, Lcom/kamcord/android/KC_s;->g:I

    invoke-virtual {v2, v4}, Landroid/widget/ImageView;->setImageResource(I)V

    const-string v4, "drawable"

    const-string v7, "kamcord_tab_background_selector"

    invoke-static {v4, v7}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v4

    invoke-virtual {v2, v4}, Landroid/widget/ImageView;->setBackgroundResource(I)V

    invoke-virtual {v2, v3}, Landroid/widget/ImageView;->setLayoutParams(Landroid/view/ViewGroup$LayoutParams;)V

    if-eqz v0, :cond_6

    if-nez v6, :cond_6

    iget v0, p0, Lcom/kamcord/android/KC_s;->h:F

    float-to-int v0, v0

    iget v3, p0, Lcom/kamcord/android/KC_s;->h:F

    float-to-int v3, v3

    invoke-virtual {v2, v0, v1, v3, v1}, Landroid/widget/ImageView;->setPadding(IIII)V

    :goto_4
    invoke-virtual {v5, v2}, La/a/a/c/KC_a;->a(Landroid/view/View;)V

    :cond_2
    :goto_5
    move-object v0, v5

    goto/16 :goto_0

    :cond_3
    move v0, v1

    goto :goto_1

    :cond_4
    move v6, v1

    goto :goto_2

    :cond_5
    new-instance v3, Landroid/widget/LinearLayout$LayoutParams;

    invoke-direct {v3, v1, v10, v7}, Landroid/widget/LinearLayout$LayoutParams;-><init>(IIF)V

    iget-object v7, p0, Lcom/kamcord/android/KC_s;->e:Landroid/app/Activity;

    invoke-virtual {v7}, Landroid/app/Activity;->getResources()Landroid/content/res/Resources;

    move-result-object v7

    const-string v8, "dimen"

    const-string v9, "kamcordTabbarItemSeparation"

    invoke-static {v8, v9}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v8

    invoke-virtual {v7, v8}, Landroid/content/res/Resources;->getDimensionPixelSize(I)I

    move-result v7

    iput v7, v3, Landroid/widget/LinearLayout$LayoutParams;->leftMargin:I

    goto :goto_3

    :cond_6
    iget v0, p0, Lcom/kamcord/android/KC_s;->h:F

    float-to-int v0, v0

    iget v3, p0, Lcom/kamcord/android/KC_s;->h:F

    float-to-int v3, v3

    invoke-virtual {v2, v1, v0, v1, v3}, Landroid/widget/ImageView;->setPadding(IIII)V

    goto :goto_4

    :cond_7
    iget-object v0, p0, Lcom/kamcord/android/KC_s;->f:Lcom/kamcord/android/KC_s$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_s$KC_a;->a:Lcom/kamcord/android/KC_s$KC_a;

    if-ne v0, v1, :cond_2

    new-instance v0, Landroid/widget/TextView;

    iget-object v1, p0, Lcom/kamcord/android/KC_s;->e:Landroid/app/Activity;

    invoke-direct {v0, v1}, Landroid/widget/TextView;-><init>(Landroid/content/Context;)V

    invoke-virtual {v5}, La/a/a/c/KC_a;->c()Ljava/lang/String;

    move-result-object v1

    sget-object v6, Ljava/util/Locale;->US:Ljava/util/Locale;

    invoke-virtual {v1, v6}, Ljava/lang/String;->toUpperCase(Ljava/util/Locale;)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Landroid/widget/TextView;->setText(Ljava/lang/CharSequence;)V

    const/16 v1, 0x11

    invoke-virtual {v0, v1}, Landroid/widget/TextView;->setGravity(I)V

    invoke-virtual {v0, v10}, Landroid/widget/TextView;->setTextColor(I)V

    invoke-virtual {v0, v4, v2}, Landroid/widget/TextView;->setTypeface(Landroid/graphics/Typeface;I)V

    const-string v1, "drawable"

    const-string v2, "kamcord_tab_background_selector"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/TextView;->setBackgroundResource(I)V

    invoke-virtual {v0, v3}, Landroid/widget/TextView;->setLayoutParams(Landroid/view/ViewGroup$LayoutParams;)V

    invoke-virtual {v5, v0}, La/a/a/c/KC_a;->a(Landroid/view/View;)V

    goto :goto_5
.end method

.method public final a(I)Lcom/kamcord/android/KC_s;
    .locals 0

    iput p1, p0, Lcom/kamcord/android/KC_s;->d:I

    return-object p0
.end method

.method public final a(Landroid/app/Activity;)Lcom/kamcord/android/KC_s;
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_s;->e:Landroid/app/Activity;

    return-object p0
.end method

.method public final a(Lcom/kamcord/android/KC_p;)Lcom/kamcord/android/KC_s;
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_s;->a:Lcom/kamcord/android/KC_p;

    return-object p0
.end method

.method public final a(Z)Lcom/kamcord/android/KC_s;
    .locals 0

    iput-boolean p1, p0, Lcom/kamcord/android/KC_s;->c:Z

    return-object p0
.end method

.method public final b(I)Lcom/kamcord/android/KC_s;
    .locals 1

    sget-object v0, Lcom/kamcord/android/KC_s$KC_a;->b:Lcom/kamcord/android/KC_s$KC_a;

    iput-object v0, p0, Lcom/kamcord/android/KC_s;->f:Lcom/kamcord/android/KC_s$KC_a;

    iput p1, p0, Lcom/kamcord/android/KC_s;->g:I

    return-object p0
.end method

.method public final c(I)Lcom/kamcord/android/KC_s;
    .locals 3

    const/4 v0, 0x1

    const/high16 v1, 0x41300000    # 11.0f

    iget-object v2, p0, Lcom/kamcord/android/KC_s;->e:Landroid/app/Activity;

    invoke-virtual {v2}, Landroid/app/Activity;->getResources()Landroid/content/res/Resources;

    move-result-object v2

    invoke-virtual {v2}, Landroid/content/res/Resources;->getDisplayMetrics()Landroid/util/DisplayMetrics;

    move-result-object v2

    invoke-static {v0, v1, v2}, Landroid/util/TypedValue;->applyDimension(IFLandroid/util/DisplayMetrics;)F

    move-result v0

    iput v0, p0, Lcom/kamcord/android/KC_s;->h:F

    return-object p0
.end method
