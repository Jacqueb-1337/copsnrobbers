.class public Lcom/kamcord/android/AspectRatioTextureView;
.super Landroid/view/TextureView;


# instance fields
.field private a:D


# direct methods
.method public constructor <init>(Landroid/content/Context;)V
    .locals 2

    invoke-direct {p0, p1}, Landroid/view/TextureView;-><init>(Landroid/content/Context;)V

    const-wide/high16 v0, -0x4010000000000000L    # -1.0

    iput-wide v0, p0, Lcom/kamcord/android/AspectRatioTextureView;->a:D

    return-void
.end method

.method public constructor <init>(Landroid/content/Context;II)V
    .locals 4

    const-wide/high16 v0, -0x4010000000000000L    # -1.0

    invoke-direct {p0, p1}, Landroid/view/TextureView;-><init>(Landroid/content/Context;)V

    iput-wide v0, p0, Lcom/kamcord/android/AspectRatioTextureView;->a:D

    if-lez p3, :cond_0

    int-to-double v0, p2

    int-to-double v2, p3

    div-double/2addr v0, v2

    iput-wide v0, p0, Lcom/kamcord/android/AspectRatioTextureView;->a:D

    :goto_0
    const/4 v0, 0x1

    invoke-virtual {p0, v0}, Lcom/kamcord/android/AspectRatioTextureView;->setDrawingCacheEnabled(Z)V

    return-void

    :cond_0
    iput-wide v0, p0, Lcom/kamcord/android/AspectRatioTextureView;->a:D

    goto :goto_0
.end method

.method public constructor <init>(Landroid/content/Context;Landroid/util/AttributeSet;)V
    .locals 2

    invoke-direct {p0, p1, p2}, Landroid/view/TextureView;-><init>(Landroid/content/Context;Landroid/util/AttributeSet;)V

    const-wide/high16 v0, -0x4010000000000000L    # -1.0

    iput-wide v0, p0, Lcom/kamcord/android/AspectRatioTextureView;->a:D

    return-void
.end method

.method public constructor <init>(Landroid/content/Context;Landroid/util/AttributeSet;I)V
    .locals 2

    invoke-direct {p0, p1, p2, p3}, Landroid/view/TextureView;-><init>(Landroid/content/Context;Landroid/util/AttributeSet;I)V

    const-wide/high16 v0, -0x4010000000000000L    # -1.0

    iput-wide v0, p0, Lcom/kamcord/android/AspectRatioTextureView;->a:D

    return-void
.end method


# virtual methods
.method public getAspectRatio()D
    .locals 2

    iget-wide v0, p0, Lcom/kamcord/android/AspectRatioTextureView;->a:D

    return-wide v0
.end method

.method protected onMeasure(II)V
    .locals 9

    const/high16 v8, 0x40000000    # 2.0f

    invoke-static {p1}, Landroid/view/View$MeasureSpec;->getSize(I)I

    move-result v2

    invoke-static {p2}, Landroid/view/View$MeasureSpec;->getSize(I)I

    move-result v0

    iget-wide v4, p0, Lcom/kamcord/android/AspectRatioTextureView;->a:D

    const-wide/16 v6, 0x0

    cmpl-double v1, v4, v6

    if-lez v1, :cond_1

    int-to-double v4, v2

    iget-wide v6, p0, Lcom/kamcord/android/AspectRatioTextureView;->a:D

    div-double/2addr v4, v6

    double-to-int v1, v4

    if-le v1, v0, :cond_0

    int-to-double v2, v0

    iget-wide v4, p0, Lcom/kamcord/android/AspectRatioTextureView;->a:D

    mul-double/2addr v2, v4

    double-to-int v1, v2

    :goto_0
    invoke-static {v1, v8}, Landroid/view/View$MeasureSpec;->makeMeasureSpec(II)I

    move-result v1

    invoke-static {v0, v8}, Landroid/view/View$MeasureSpec;->makeMeasureSpec(II)I

    move-result v0

    invoke-super {p0, v1, v0}, Landroid/view/TextureView;->onMeasure(II)V

    return-void

    :cond_0
    move v0, v1

    move v1, v2

    goto :goto_0

    :cond_1
    move v1, v2

    goto :goto_0
.end method

.method public setAspectRatio(II)V
    .locals 4

    if-lez p2, :cond_0

    int-to-double v0, p1

    int-to-double v2, p2

    div-double/2addr v0, v2

    iput-wide v0, p0, Lcom/kamcord/android/AspectRatioTextureView;->a:D

    :goto_0
    invoke-virtual {p0}, Lcom/kamcord/android/AspectRatioTextureView;->requestLayout()V

    return-void

    :cond_0
    const-wide/high16 v0, -0x4010000000000000L    # -1.0

    iput-wide v0, p0, Lcom/kamcord/android/AspectRatioTextureView;->a:D

    goto :goto_0
.end method
