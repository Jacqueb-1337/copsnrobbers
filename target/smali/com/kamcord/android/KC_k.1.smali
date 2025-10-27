.class public final Lcom/kamcord/android/KC_k;
.super La/a/a/a/KC_e;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/KC_k$KC_a;
    }
.end annotation


# instance fields
.field M:Lcom/kamcord/android/KamcordActivity;

.field private N:Landroid/view/View;

.field private O:Landroid/widget/EditText;


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, La/a/a/a/KC_e;-><init>()V

    return-void
.end method

.method static synthetic a(Lcom/kamcord/android/KC_k;)Landroid/widget/EditText;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_k;->O:Landroid/widget/EditText;

    return-object v0
.end method

.method static synthetic a(Lcom/kamcord/android/KC_k;Ljava/lang/String;)V
    .locals 3

    invoke-virtual {p1}, Ljava/lang/String;->length()I

    move-result v0

    if-nez v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_k;->O:Landroid/widget/EditText;

    const-string v1, "kamcordEnterUsernameOrEmail"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Landroid/widget/EditText;->setError(Ljava/lang/CharSequence;)V

    :goto_0
    return-void

    :cond_0
    invoke-static {}, La/a/a/c/KC_a;->a()Z

    move-result v0

    if-nez v0, :cond_1

    new-instance v0, Lcom/kamcord/android/KC_x;

    invoke-direct {v0}, Lcom/kamcord/android/KC_x;-><init>()V

    invoke-virtual {p0}, Lcom/kamcord/android/KC_k;->h()La/a/a/a/KC_f;

    move-result-object v1

    invoke-virtual {v1}, La/a/a/a/KC_f;->getFragmentManager()Landroid/app/FragmentManager;

    move-result-object v1

    const/4 v2, 0x0

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/android/KC_x;->show(Landroid/app/FragmentManager;Ljava/lang/String;)V

    goto :goto_0

    :cond_1
    new-instance v0, Lcom/kamcord/android/KC_k$KC_a;

    invoke-direct {v0, p0}, Lcom/kamcord/android/KC_k$KC_a;-><init>(Lcom/kamcord/android/KC_k;)V

    const/4 v1, 0x1

    new-array v1, v1, [Ljava/lang/String;

    const/4 v2, 0x0

    aput-object p1, v1, v2

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_k$KC_a;->execute([Ljava/lang/Object;)Landroid/os/AsyncTask;

    goto :goto_0
.end method


# virtual methods
.method public final a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
    .locals 3

    const-string v0, "layout"

    const-string v1, "z_kamcord_fragment_forgot_password"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    const/4 v1, 0x0

    invoke-virtual {p1, v0, p2, v1}, Landroid/view/LayoutInflater;->inflate(ILandroid/view/ViewGroup;Z)Landroid/view/View;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_k;->N:Landroid/view/View;

    iget-object v0, p0, Lcom/kamcord/android/KC_k;->N:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "usernameOrEmail"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/EditText;

    iput-object v0, p0, Lcom/kamcord/android/KC_k;->O:Landroid/widget/EditText;

    iget-object v0, p0, Lcom/kamcord/android/KC_k;->N:Landroid/view/View;

    iput-object v0, p0, Lcom/kamcord/android/KC_k;->N:Landroid/view/View;

    iget-object v0, p0, Lcom/kamcord/android/KC_k;->O:Landroid/widget/EditText;

    new-instance v1, Lcom/kamcord/android/KC_k$1;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_k$1;-><init>(Lcom/kamcord/android/KC_k;)V

    invoke-virtual {v0, v1}, Landroid/widget/EditText;->setOnEditorActionListener(Landroid/widget/TextView$OnEditorActionListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_k;->N:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "resetPassword"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/Button;

    new-instance v1, Lcom/kamcord/android/KC_k$2;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_k$2;-><init>(Lcom/kamcord/android/KC_k;)V

    invoke-virtual {v0, v1}, Landroid/widget/Button;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_k;->N:Landroid/view/View;

    return-object v0
.end method

.method public final a(Landroid/app/Activity;)V
    .locals 0

    invoke-super {p0, p1}, La/a/a/a/KC_e;->a(Landroid/app/Activity;)V

    check-cast p1, Lcom/kamcord/android/KamcordActivity;

    iput-object p1, p0, Lcom/kamcord/android/KC_k;->M:Lcom/kamcord/android/KamcordActivity;

    return-void
.end method
