.class final La/a/a/a/KC_f$2;
.super Ljava/lang/Object;
.source "SourceFile"

# interfaces
.implements La/a/a/a/KC_g;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = La/a/a/a/KC_f;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:La/a/a/a/KC_f;


# direct methods
.method constructor <init>(La/a/a/a/KC_f;)V
    .locals 0

    .prologue
    .line 107
    iput-object p1, p0, La/a/a/a/KC_f$2;->a:La/a/a/a/KC_f;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final a(I)Landroid/view/View;
    .locals 1

    .prologue
    .line 110
    iget-object v0, p0, La/a/a/a/KC_f$2;->a:La/a/a/a/KC_f;

    invoke-virtual {v0, p1}, La/a/a/a/KC_f;->findViewById(I)Landroid/view/View;

    move-result-object v0

    return-object v0
.end method
