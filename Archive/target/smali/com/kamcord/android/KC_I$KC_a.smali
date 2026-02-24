.class final Lcom/kamcord/android/KC_I$KC_a;
.super Landroid/app/DialogFragment;


# annotations
.annotation build Landroid/annotation/SuppressLint;
    value = {
        "ValidFragment"
    }
.end annotation

.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/KC_I;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = "KC_a"
.end annotation


# instance fields
.field final synthetic a:Lcom/kamcord/android/KC_I;

.field private b:Lcom/kamcord/android/b/KC_e;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_I;Lcom/kamcord/android/b/KC_e;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_I$KC_a;->a:Lcom/kamcord/android/KC_I;

    invoke-direct {p0}, Landroid/app/DialogFragment;-><init>()V

    iput-object p2, p0, Lcom/kamcord/android/KC_I$KC_a;->b:Lcom/kamcord/android/b/KC_e;

    return-void
.end method

.method static synthetic a(Lcom/kamcord/android/KC_I$KC_a;)Lcom/kamcord/android/b/KC_e;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_I$KC_a;->b:Lcom/kamcord/android/b/KC_e;

    return-object v0
.end method


# virtual methods
.method public final onCreateDialog(Landroid/os/Bundle;)Landroid/app/Dialog;
    .locals 6

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "kamcordConfirm"

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v1, p0, Lcom/kamcord/android/KC_I$KC_a;->b:Lcom/kamcord/android/b/KC_e;

    invoke-virtual {v1}, Lcom/kamcord/android/b/KC_e;->d()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, "SignOut"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    const-string v1, "kamcordAreYouSure"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    new-instance v2, Landroid/app/AlertDialog$Builder;

    invoke-virtual {p0}, Lcom/kamcord/android/KC_I$KC_a;->getActivity()Landroid/app/Activity;

    move-result-object v3

    invoke-direct {v2, v3}, Landroid/app/AlertDialog$Builder;-><init>(Landroid/content/Context;)V

    const-string v3, "kamcordSignOut"

    invoke-static {v3}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    new-instance v4, Lcom/kamcord/android/KC_I$KC_a$1;

    invoke-direct {v4, p0}, Lcom/kamcord/android/KC_I$KC_a$1;-><init>(Lcom/kamcord/android/KC_I$KC_a;)V

    invoke-virtual {v2, v3, v4}, Landroid/app/AlertDialog$Builder;->setPositiveButton(Ljava/lang/CharSequence;Landroid/content/DialogInterface$OnClickListener;)Landroid/app/AlertDialog$Builder;

    move-result-object v3

    const/high16 v4, 0x1040000

    const/4 v5, 0x0

    invoke-virtual {v3, v4, v5}, Landroid/app/AlertDialog$Builder;->setNegativeButton(ILandroid/content/DialogInterface$OnClickListener;)Landroid/app/AlertDialog$Builder;

    move-result-object v3

    invoke-virtual {v3, v0}, Landroid/app/AlertDialog$Builder;->setTitle(Ljava/lang/CharSequence;)Landroid/app/AlertDialog$Builder;

    move-result-object v0

    invoke-virtual {v0, v1}, Landroid/app/AlertDialog$Builder;->setMessage(Ljava/lang/CharSequence;)Landroid/app/AlertDialog$Builder;

    invoke-virtual {v2}, Landroid/app/AlertDialog$Builder;->create()Landroid/app/AlertDialog;

    move-result-object v0

    return-object v0
.end method
