.class final Lcom/kamcord/android/core/KC_d;
.super Lcom/kamcord/android/KC_ac;


# instance fields
.field private b:Lcom/kamcord/android/core/KC_T;


# direct methods
.method constructor <init>(Landroid/os/Looper;Lcom/kamcord/android/core/KC_T;)V
    .locals 0

    invoke-direct {p0, p1}, Lcom/kamcord/android/KC_ac;-><init>(Landroid/os/Looper;)V

    iput-object p2, p0, Lcom/kamcord/android/core/KC_d;->b:Lcom/kamcord/android/core/KC_T;

    return-void
.end method


# virtual methods
.method public final a(Landroid/os/Message;)V
    .locals 4

    iget v0, p1, Landroid/os/Message;->what:I

    packed-switch v0, :pswitch_data_0

    :goto_0
    return-void

    :pswitch_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_d;->b:Lcom/kamcord/android/core/KC_T;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_T;->f()V

    goto :goto_0

    :pswitch_1
    iget-object v0, p0, Lcom/kamcord/android/core/KC_d;->b:Lcom/kamcord/android/core/KC_T;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_T;->e()V

    goto :goto_0

    :pswitch_2
    iget-object v1, p0, Lcom/kamcord/android/core/KC_d;->b:Lcom/kamcord/android/core/KC_T;

    iget-object v0, p1, Landroid/os/Message;->obj:Ljava/lang/Object;

    check-cast v0, Ljava/lang/Double;

    invoke-virtual {v0}, Ljava/lang/Double;->doubleValue()D

    move-result-wide v2

    invoke-virtual {v1, v2, v3}, Lcom/kamcord/android/core/KC_T;->a(D)V

    goto :goto_0

    :pswitch_data_0
    .packed-switch 0x1
        :pswitch_0
        :pswitch_1
        :pswitch_2
    .end packed-switch
.end method

.method public final b(Landroid/os/Message;)V
    .locals 1

    iget v0, p1, Landroid/os/Message;->what:I

    packed-switch v0, :pswitch_data_0

    :goto_0
    return-void

    :pswitch_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_d;->b:Lcom/kamcord/android/core/KC_T;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_T;->g()V

    goto :goto_0

    :pswitch_1
    iget-object v0, p0, Lcom/kamcord/android/core/KC_d;->b:Lcom/kamcord/android/core/KC_T;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_T;->e()V

    goto :goto_0

    :pswitch_data_0
    .packed-switch 0x1
        :pswitch_0
        :pswitch_1
    .end packed-switch
.end method
