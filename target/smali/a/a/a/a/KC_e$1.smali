.class final La/a/a/a/KC_e$1;
.super Ljava/lang/Object;
.source "SourceFile"

# interfaces
.implements La/a/a/a/KC_g;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = La/a/a/a/KC_e;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:La/a/a/a/KC_e;


# direct methods
.method constructor <init>(La/a/a/a/KC_e;)V
    .locals 0

    .prologue
    .line 1465
    iput-object p1, p0, La/a/a/a/KC_e$1;->a:La/a/a/a/KC_e;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final a(I)Landroid/view/View;
    .locals 2

    .prologue
    .line 1468
    iget-object v0, p0, La/a/a/a/KC_e$1;->a:La/a/a/a/KC_e;

    iget-object v0, v0, La/a/a/a/KC_e;->H:Landroid/view/View;

    if-nez v0, :cond_0

    .line 1469
    new-instance v0, Ljava/lang/IllegalStateException;

    const-string v1, "Fragment does not have a view"

    invoke-direct {v0, v1}, Ljava/lang/IllegalStateException;-><init>(Ljava/lang/String;)V

    throw v0

    .line 1471
    :cond_0
    iget-object v0, p0, La/a/a/a/KC_e$1;->a:La/a/a/a/KC_e;

    iget-object v0, v0, La/a/a/a/KC_e;->H:Landroid/view/View;

    invoke-virtual {v0, p1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    return-object v0
.end method
