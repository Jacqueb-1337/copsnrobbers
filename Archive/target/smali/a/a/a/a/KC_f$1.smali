.class final La/a/a/a/KC_f$1;
.super Landroid/os/Handler;
.source "SourceFile"


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
    .line 87
    iput-object p1, p0, La/a/a/a/KC_f$1;->a:La/a/a/a/KC_f;

    invoke-direct {p0}, Landroid/os/Handler;-><init>()V

    return-void
.end method


# virtual methods
.method public final handleMessage(Landroid/os/Message;)V
    .locals 2
    .param p1, "msg"    # Landroid/os/Message;

    .prologue
    .line 90
    iget v0, p1, Landroid/os/Message;->what:I

    packed-switch v0, :pswitch_data_0

    .line 101
    invoke-super {p0, p1}, Landroid/os/Handler;->handleMessage(Landroid/os/Message;)V

    .line 103
    :cond_0
    :goto_0
    return-void

    .line 92
    :pswitch_0
    iget-object v0, p0, La/a/a/a/KC_f$1;->a:La/a/a/a/KC_f;

    iget-boolean v0, v0, La/a/a/a/KC_f;->c:Z

    if-eqz v0, :cond_0

    .line 93
    iget-object v0, p0, La/a/a/a/KC_f$1;->a:La/a/a/a/KC_f;

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, La/a/a/a/KC_f;->a(Z)V

    goto :goto_0

    .line 97
    :pswitch_1
    iget-object v0, p0, La/a/a/a/KC_f$1;->a:La/a/a/a/KC_f;

    invoke-virtual {v0}, La/a/a/a/KC_f;->onResumeFragments()V

    .line 98
    iget-object v0, p0, La/a/a/a/KC_f$1;->a:La/a/a/a/KC_f;

    iget-object v0, v0, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->d()Z

    goto :goto_0

    .line 90
    nop

    :pswitch_data_0
    .packed-switch 0x1
        :pswitch_0
        :pswitch_1
    .end packed-switch
.end method
