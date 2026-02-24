.class final La/a/a/e/KC_f$KC_b;
.super Ljava/lang/Object;
.source "SourceFile"

# interfaces
.implements La/a/a/e/KC_f$KC_c;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = La/a/a/e/KC_f;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x8
    name = "KC_b"
.end annotation


# direct methods
.method constructor <init>()V
    .locals 0

    .prologue
    .line 51
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final a(Landroid/view/VelocityTracker;I)F
    .locals 1

    .prologue
    .line 54
    invoke-virtual {p1, p2}, Landroid/view/VelocityTracker;->getXVelocity(I)F

    move-result v0

    return v0
.end method
