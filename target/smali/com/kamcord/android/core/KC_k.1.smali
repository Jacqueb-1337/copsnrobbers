.class final Lcom/kamcord/android/core/KC_k;
.super Lcom/kamcord/android/KC_ac;


# instance fields
.field private b:Lcom/kamcord/android/core/KC_j;

.field private c:J


# direct methods
.method constructor <init>(Landroid/os/Looper;Lcom/kamcord/android/core/KC_j;)V
    .locals 2

    invoke-direct {p0, p1}, Lcom/kamcord/android/KC_ac;-><init>(Landroid/os/Looper;)V

    const-wide/16 v0, 0x0

    iput-wide v0, p0, Lcom/kamcord/android/core/KC_k;->c:J

    iput-object p2, p0, Lcom/kamcord/android/core/KC_k;->b:Lcom/kamcord/android/core/KC_j;

    return-void
.end method


# virtual methods
.method public final a(Landroid/os/Message;)V
    .locals 6

    iget v0, p1, Landroid/os/Message;->what:I

    packed-switch v0, :pswitch_data_0

    :goto_0
    return-void

    :pswitch_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_k;->b:Lcom/kamcord/android/core/KC_j;

    invoke-interface {v0}, Lcom/kamcord/android/core/KC_j;->a()V

    goto :goto_0

    :pswitch_1
    invoke-static {}, Ljava/lang/System;->nanoTime()J

    move-result-wide v0

    iget-wide v2, p0, Lcom/kamcord/android/core/KC_k;->c:J

    sub-long v2, v0, v2

    const-wide v4, 0x2540be400L

    cmp-long v2, v2, v4

    if-lez v2, :cond_0

    const/4 v2, 0x1

    invoke-static {v2}, Lcom/kamcord/android/Kamcord;->setCurrentThreadAffinity(I)V

    iput-wide v0, p0, Lcom/kamcord/android/core/KC_k;->c:J

    :cond_0
    iget-object v0, p1, Landroid/os/Message;->obj:Ljava/lang/Object;

    check-cast v0, Lcom/kamcord/android/core/KC_g;

    iget-object v1, v0, Lcom/kamcord/android/core/KC_g;->a:Lcom/kamcord/android/core/KC_x;

    const/4 v2, 0x0

    iget-boolean v3, v0, Lcom/kamcord/android/core/KC_g;->b:Z

    iget-object v4, v0, Lcom/kamcord/android/core/KC_g;->c:Lcom/kamcord/android/core/KC_y;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_g;->d:Lcom/kamcord/android/core/KC_y;

    invoke-virtual {v1, v2, v3, v4, v0}, Lcom/kamcord/android/core/KC_x;->a(ZZLcom/kamcord/android/core/KC_y;Lcom/kamcord/android/core/KC_y;)V

    goto :goto_0

    :pswitch_2
    iget-object v1, p0, Lcom/kamcord/android/core/KC_k;->b:Lcom/kamcord/android/core/KC_j;

    iget-object v0, p1, Landroid/os/Message;->obj:Ljava/lang/Object;

    check-cast v0, Lcom/kamcord/android/core/KC_F;

    invoke-interface {v1, v0}, Lcom/kamcord/android/core/KC_j;->a(Lcom/kamcord/android/core/KC_F;)V

    goto :goto_0

    :pswitch_3
    iget-object v0, p0, Lcom/kamcord/android/core/KC_k;->b:Lcom/kamcord/android/core/KC_j;

    invoke-interface {v0}, Lcom/kamcord/android/core/KC_j;->b()V

    goto :goto_0

    :pswitch_data_0
    .packed-switch 0x1
        :pswitch_0
        :pswitch_1
        :pswitch_2
        :pswitch_3
    .end packed-switch
.end method

.method public final b(Landroid/os/Message;)V
    .locals 1

    iget v0, p1, Landroid/os/Message;->what:I

    packed-switch v0, :pswitch_data_0

    :goto_0
    return-void

    :pswitch_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_k;->b:Lcom/kamcord/android/core/KC_j;

    invoke-interface {v0}, Lcom/kamcord/android/core/KC_j;->b()V

    goto :goto_0

    :pswitch_data_0
    .packed-switch 0x4
        :pswitch_0
    .end packed-switch
.end method
