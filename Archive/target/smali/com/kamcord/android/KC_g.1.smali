.class final Lcom/kamcord/android/KC_g;
.super Landroid/app/DialogFragment;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/KC_g$2;
    }
.end annotation


# instance fields
.field private a:Lcom/kamcord/android/KC_f;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_f;)V
    .locals 0

    invoke-direct {p0}, Landroid/app/DialogFragment;-><init>()V

    iput-object p1, p0, Lcom/kamcord/android/KC_g;->a:Lcom/kamcord/android/KC_f;

    return-void
.end method

.method static synthetic a(Lcom/kamcord/android/KC_g;)Lcom/kamcord/android/KC_f;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_g;->a:Lcom/kamcord/android/KC_f;

    return-object v0
.end method


# virtual methods
.method public final onCreateDialog(Landroid/os/Bundle;)Landroid/app/Dialog;
    .locals 5

    const-string v0, "kamcordSuccess"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    const-string v0, ""

    sget-object v2, Lcom/kamcord/android/KC_g$2;->a:[I

    iget-object v3, p0, Lcom/kamcord/android/KC_g;->a:Lcom/kamcord/android/KC_f;

    iget-object v3, v3, Lcom/kamcord/android/KC_f;->N:Lcom/kamcord/android/KC_f$KC_a;

    invoke-virtual {v3}, Lcom/kamcord/android/KC_f$KC_a;->ordinal()I

    move-result v3

    aget v2, v2, v3

    packed-switch v2, :pswitch_data_0

    :goto_0
    new-instance v2, Landroid/app/AlertDialog$Builder;

    invoke-virtual {p0}, Lcom/kamcord/android/KC_g;->getActivity()Landroid/app/Activity;

    move-result-object v3

    invoke-direct {v2, v3}, Landroid/app/AlertDialog$Builder;-><init>(Landroid/content/Context;)V

    const-string v3, "kamcordOk"

    invoke-static {v3}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    new-instance v4, Lcom/kamcord/android/KC_g$1;

    invoke-direct {v4, p0}, Lcom/kamcord/android/KC_g$1;-><init>(Lcom/kamcord/android/KC_g;)V

    invoke-virtual {v2, v3, v4}, Landroid/app/AlertDialog$Builder;->setNeutralButton(Ljava/lang/CharSequence;Landroid/content/DialogInterface$OnClickListener;)Landroid/app/AlertDialog$Builder;

    move-result-object v3

    invoke-virtual {v3, v1}, Landroid/app/AlertDialog$Builder;->setTitle(Ljava/lang/CharSequence;)Landroid/app/AlertDialog$Builder;

    move-result-object v1

    invoke-virtual {v1, v0}, Landroid/app/AlertDialog$Builder;->setMessage(Ljava/lang/CharSequence;)Landroid/app/AlertDialog$Builder;

    invoke-virtual {v2}, Landroid/app/AlertDialog$Builder;->create()Landroid/app/AlertDialog;

    move-result-object v0

    return-object v0

    :pswitch_0
    const-string v0, "kamcordSuccessfullyChangedEmail"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    goto :goto_0

    :pswitch_1
    const-string v0, "kamcordSuccessfullyChangedPassword"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    goto :goto_0

    nop

    :pswitch_data_0
    .packed-switch 0x1
        :pswitch_0
        :pswitch_1
    .end packed-switch
.end method
