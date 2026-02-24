.class final Lcom/kamcord/android/KC_O;
.super La/a/a/a/KC_d;


# instance fields
.field M:Lcom/kamcord/android/KamcordActivity;

.field private N:Ljava/lang/String;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KamcordActivity;Ljava/lang/String;)V
    .locals 1

    invoke-direct {p0}, La/a/a/a/KC_d;-><init>()V

    const-string v0, ""

    iput-object v0, p0, Lcom/kamcord/android/KC_O;->N:Ljava/lang/String;

    iput-object p1, p0, Lcom/kamcord/android/KC_O;->M:Lcom/kamcord/android/KamcordActivity;

    iput-object p2, p0, Lcom/kamcord/android/KC_O;->N:Ljava/lang/String;

    return-void
.end method


# virtual methods
.method public final b()Landroid/app/Dialog;
    .locals 4

    new-instance v0, Landroid/app/AlertDialog$Builder;

    invoke-virtual {p0}, Lcom/kamcord/android/KC_O;->h()La/a/a/a/KC_f;

    move-result-object v1

    invoke-direct {v0, v1}, Landroid/app/AlertDialog$Builder;-><init>(Landroid/content/Context;)V

    const-string v1, "kamcordSignIn"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Landroid/app/AlertDialog$Builder;->setTitle(Ljava/lang/CharSequence;)Landroid/app/AlertDialog$Builder;

    move-result-object v1

    iget-object v2, p0, Lcom/kamcord/android/KC_O;->N:Ljava/lang/String;

    invoke-virtual {v1, v2}, Landroid/app/AlertDialog$Builder;->setMessage(Ljava/lang/CharSequence;)Landroid/app/AlertDialog$Builder;

    move-result-object v1

    const-string v2, "kamcordOk"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    const/4 v3, 0x0

    invoke-virtual {v1, v2, v3}, Landroid/app/AlertDialog$Builder;->setNegativeButton(Ljava/lang/CharSequence;Landroid/content/DialogInterface$OnClickListener;)Landroid/app/AlertDialog$Builder;

    move-result-object v1

    const-string v2, "kamcordSignIn"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    new-instance v3, Lcom/kamcord/android/KC_O$2;

    invoke-direct {v3, p0}, Lcom/kamcord/android/KC_O$2;-><init>(Lcom/kamcord/android/KC_O;)V

    invoke-virtual {v1, v2, v3}, Landroid/app/AlertDialog$Builder;->setNeutralButton(Ljava/lang/CharSequence;Landroid/content/DialogInterface$OnClickListener;)Landroid/app/AlertDialog$Builder;

    move-result-object v1

    const-string v2, "kamcordCreateProfile"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    new-instance v3, Lcom/kamcord/android/KC_O$1;

    invoke-direct {v3, p0}, Lcom/kamcord/android/KC_O$1;-><init>(Lcom/kamcord/android/KC_O;)V

    invoke-virtual {v1, v2, v3}, Landroid/app/AlertDialog$Builder;->setPositiveButton(Ljava/lang/CharSequence;Landroid/content/DialogInterface$OnClickListener;)Landroid/app/AlertDialog$Builder;

    invoke-virtual {v0}, Landroid/app/AlertDialog$Builder;->create()Landroid/app/AlertDialog;

    move-result-object v0

    return-object v0
.end method
