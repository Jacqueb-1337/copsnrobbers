.class final Lcom/kamcord/android/core/KC_t;
.super Lcom/kamcord/android/KC_ac;


# instance fields
.field private b:Lcom/kamcord/android/core/KC_u;


# direct methods
.method constructor <init>(Landroid/os/Looper;Lcom/kamcord/android/core/KC_u;)V
    .locals 0

    invoke-direct {p0, p1}, Lcom/kamcord/android/KC_ac;-><init>(Landroid/os/Looper;)V

    iput-object p2, p0, Lcom/kamcord/android/core/KC_t;->b:Lcom/kamcord/android/core/KC_u;

    return-void
.end method


# virtual methods
.method public final a(Landroid/os/Message;)V
    .locals 2

    const/4 v1, 0x1

    iget v0, p1, Landroid/os/Message;->what:I

    packed-switch v0, :pswitch_data_0

    :goto_0
    return-void

    :pswitch_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_t;->b:Lcom/kamcord/android/core/KC_u;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_u;->n()V

    goto :goto_0

    :pswitch_1
    iget-object v0, p0, Lcom/kamcord/android/core/KC_t;->b:Lcom/kamcord/android/core/KC_u;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_u;->o()V

    goto :goto_0

    :pswitch_2
    iget-object v0, p0, Lcom/kamcord/android/core/KC_t;->b:Lcom/kamcord/android/core/KC_u;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_u;->p()V

    goto :goto_0

    :pswitch_3
    iget-object v0, p0, Lcom/kamcord/android/core/KC_t;->b:Lcom/kamcord/android/core/KC_u;

    invoke-virtual {v0, v1}, Lcom/kamcord/android/core/KC_u;->a(Z)V

    goto :goto_0

    :pswitch_4
    iget-object v0, p0, Lcom/kamcord/android/core/KC_t;->b:Lcom/kamcord/android/core/KC_u;

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Lcom/kamcord/android/core/KC_u;->a(I)V

    goto :goto_0

    :pswitch_5
    iget-object v0, p0, Lcom/kamcord/android/core/KC_t;->b:Lcom/kamcord/android/core/KC_u;

    invoke-virtual {v0, v1}, Lcom/kamcord/android/core/KC_u;->a(I)V

    goto :goto_0

    :pswitch_6
    const-string v0, "Exception caused kamcord worker thread to shut down."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->fatalError(Ljava/lang/String;)V

    goto :goto_0

    :pswitch_data_0
    .packed-switch 0x1
        :pswitch_0
        :pswitch_1
        :pswitch_2
        :pswitch_3
        :pswitch_4
        :pswitch_5
        :pswitch_6
    .end packed-switch
.end method

.method public final b(Landroid/os/Message;)V
    .locals 2

    iget v0, p1, Landroid/os/Message;->what:I

    packed-switch v0, :pswitch_data_0

    :goto_0
    :pswitch_0
    return-void

    :pswitch_1
    iget-object v0, p0, Lcom/kamcord/android/core/KC_t;->b:Lcom/kamcord/android/core/KC_u;

    const/4 v1, 0x1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/core/KC_u;->a(Z)V

    goto :goto_0

    :pswitch_2
    const-string v0, "Exception caused kamcord worker thread to shut down."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->fatalError(Ljava/lang/String;)V

    goto :goto_0

    nop

    :pswitch_data_0
    .packed-switch 0x4
        :pswitch_1
        :pswitch_0
        :pswitch_0
        :pswitch_2
    .end packed-switch
.end method
