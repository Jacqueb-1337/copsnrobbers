.class final La/a/a/e/KC_i$3;
.super Ljava/lang/Object;
.source "SourceFile"

# interfaces
.implements Ljava/lang/Runnable;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = La/a/a/e/KC_i;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:La/a/a/e/KC_i;


# direct methods
.method constructor <init>(La/a/a/e/KC_i;)V
    .locals 0

    .prologue
    .line 246
    iput-object p1, p0, La/a/a/e/KC_i$3;->a:La/a/a/e/KC_i;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final run()V
    .locals 2

    .prologue
    .line 248
    iget-object v0, p0, La/a/a/e/KC_i$3;->a:La/a/a/e/KC_i;

    const/4 v1, 0x0

    invoke-static {v0, v1}, La/a/a/e/KC_i;->a(La/a/a/e/KC_i;I)V

    .line 249
    iget-object v0, p0, La/a/a/e/KC_i$3;->a:La/a/a/e/KC_i;

    invoke-virtual {v0}, La/a/a/e/KC_i;->b()V

    .line 250
    return-void
.end method
