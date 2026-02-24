.class La/a/a/e/KC_g$KC_c;
.super La/a/a/e/KC_g$KC_b;
.source "SourceFile"


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = La/a/a/e/KC_g;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x8
    name = "KC_c"
.end annotation


# direct methods
.method constructor <init>()V
    .locals 0

    .prologue
    .line 669
    invoke-direct {p0}, La/a/a/e/KC_g$KC_b;-><init>()V

    return-void
.end method


# virtual methods
.method final a()J
    .locals 2

    .prologue
    .line 672
    invoke-static {}, Landroid/animation/ValueAnimator;->getFrameDelay()J

    move-result-wide v0

    return-wide v0
.end method

.method public final a(Landroid/view/View;ILandroid/graphics/Paint;)V
    .locals 0

    .prologue
    .line 680
    invoke-virtual {p1, p2, p3}, Landroid/view/View;->setLayerType(ILandroid/graphics/Paint;)V

    .line 681
    return-void
.end method
