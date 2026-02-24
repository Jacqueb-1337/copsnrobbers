.class public final Lcom/kamcord/android/KC_P;
.super La/a/a/a/KC_e;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/KC_P$KC_a;,
        Lcom/kamcord/android/KC_P$KC_b;
    }
.end annotation


# instance fields
.field private M:Lcom/kamcord/android/KamcordActivity;

.field private N:Landroid/view/View;

.field private O:Landroid/widget/EditText;

.field private P:Landroid/widget/EditText;

.field private Q:Landroid/widget/Button;


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, La/a/a/a/KC_e;-><init>()V

    return-void
.end method

.method static synthetic a(Lcom/kamcord/android/KC_P;)Landroid/widget/EditText;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_P;->O:Landroid/widget/EditText;

    return-object v0
.end method

.method static synthetic b(Lcom/kamcord/android/KC_P;)Landroid/widget/EditText;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_P;->P:Landroid/widget/EditText;

    return-object v0
.end method

.method static synthetic c(Lcom/kamcord/android/KC_P;)Landroid/view/View;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_P;->N:Landroid/view/View;

    return-object v0
.end method

.method static synthetic d(Lcom/kamcord/android/KC_P;)Lcom/kamcord/android/KamcordActivity;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_P;->M:Lcom/kamcord/android/KamcordActivity;

    return-object v0
.end method


# virtual methods
.method public final a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
    .locals 5

    const-string v0, "layout"

    const-string v1, "z_kamcord_fragment_sign_in"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    const/4 v1, 0x0

    invoke-virtual {p1, v0, p2, v1}, Landroid/view/LayoutInflater;->inflate(ILandroid/view/ViewGroup;Z)Landroid/view/View;

    move-result-object v1

    const-string v0, "id"

    const-string v2, "forgotPassword"

    invoke-static {v0, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {v1, v0}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/CustomWebView;

    new-instance v2, Lcom/kamcord/android/KC_P$KC_a;

    invoke-direct {v2, p0}, Lcom/kamcord/android/KC_P$KC_a;-><init>(Lcom/kamcord/android/KC_P;)V

    invoke-virtual {v0, v2}, Lcom/kamcord/android/CustomWebView;->setWebViewClient(Landroid/webkit/WebViewClient;)V

    const-string v2, "kamcordActivityBackground"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord;->getColor(Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v0, v2}, Lcom/kamcord/android/CustomWebView;->setBackgroundColor(I)V

    const-string v2, "background-color : transparent;color: white;font-family: \'Helvetica Neue\', sans-serif;text-align: center;font-weight : light;font-size : 12px;-webkit-touch-callout: none;-webkit-user-select: none;"

    new-instance v3, Ljava/lang/StringBuilder;

    const-string v4, "<html><body style=\""

    invoke-direct {v3, v4}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v3, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "\"><a style=\"color : #8c8f91;\" href=\"kc-forgotpassword://{}\">"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "kamcordForgotYourPassword"

    invoke-static {v3}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "</a></body>"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "</html>"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    const-string v3, "text/html; charset=utf-8"

    const-string v4, "UTF-8"

    invoke-virtual {v0, v2, v3, v4}, Lcom/kamcord/android/CustomWebView;->loadData(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V

    iput-object v1, p0, Lcom/kamcord/android/KC_P;->N:Landroid/view/View;

    iget-object v0, p0, Lcom/kamcord/android/KC_P;->N:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "username"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/EditText;

    iput-object v0, p0, Lcom/kamcord/android/KC_P;->O:Landroid/widget/EditText;

    iget-object v0, p0, Lcom/kamcord/android/KC_P;->N:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "password"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/EditText;

    iput-object v0, p0, Lcom/kamcord/android/KC_P;->P:Landroid/widget/EditText;

    iget-object v0, p0, Lcom/kamcord/android/KC_P;->P:Landroid/widget/EditText;

    new-instance v1, Lcom/kamcord/android/KC_P$1;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_P$1;-><init>(Lcom/kamcord/android/KC_P;)V

    invoke-virtual {v0, v1}, Landroid/widget/EditText;->setOnEditorActionListener(Landroid/widget/TextView$OnEditorActionListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_P;->N:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "signIn"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/Button;

    iput-object v0, p0, Lcom/kamcord/android/KC_P;->Q:Landroid/widget/Button;

    iget-object v0, p0, Lcom/kamcord/android/KC_P;->Q:Landroid/widget/Button;

    new-instance v1, Lcom/kamcord/android/KC_P$2;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_P$2;-><init>(Lcom/kamcord/android/KC_P;)V

    invoke-virtual {v0, v1}, Landroid/widget/Button;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_P;->N:Landroid/view/View;

    return-object v0
.end method

.method public final a(Landroid/app/Activity;)V
    .locals 0

    invoke-super {p0, p1}, La/a/a/a/KC_e;->a(Landroid/app/Activity;)V

    check-cast p1, Lcom/kamcord/android/KamcordActivity;

    iput-object p1, p0, Lcom/kamcord/android/KC_P;->M:Lcom/kamcord/android/KamcordActivity;

    return-void
.end method

.method public final a(Ljava/lang/String;Ljava/lang/String;)V
    .locals 4

    const/4 v2, 0x1

    const/4 v1, 0x0

    invoke-virtual {p1}, Ljava/lang/String;->length()I

    move-result v0

    if-nez v0, :cond_3

    iget-object v0, p0, Lcom/kamcord/android/KC_P;->O:Landroid/widget/EditText;

    const-string v3, "kamcordUsernameEnter"

    invoke-static {v3}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v0, v3}, Landroid/widget/EditText;->setError(Ljava/lang/CharSequence;)V

    move v0, v1

    :goto_0
    invoke-virtual {p2}, Ljava/lang/String;->length()I

    move-result v3

    if-nez v3, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_P;->P:Landroid/widget/EditText;

    const-string v3, "kamcordPasswordEnter"

    invoke-static {v3}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v0, v3}, Landroid/widget/EditText;->setError(Ljava/lang/CharSequence;)V

    move v0, v1

    :cond_0
    if-eqz v0, :cond_1

    invoke-static {}, La/a/a/c/KC_a;->a()Z

    move-result v0

    if-nez v0, :cond_2

    new-instance v0, Lcom/kamcord/android/KC_x;

    invoke-direct {v0}, Lcom/kamcord/android/KC_x;-><init>()V

    iget-object v1, p0, Lcom/kamcord/android/KC_P;->M:Lcom/kamcord/android/KamcordActivity;

    invoke-virtual {v1}, Lcom/kamcord/android/KamcordActivity;->getFragmentManager()Landroid/app/FragmentManager;

    move-result-object v1

    const/4 v2, 0x0

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/android/KC_x;->show(Landroid/app/FragmentManager;Ljava/lang/String;)V

    :cond_1
    :goto_1
    return-void

    :cond_2
    iget-object v0, p0, Lcom/kamcord/android/KC_P;->N:Landroid/view/View;

    invoke-virtual {v0}, Landroid/view/View;->getParent()Landroid/view/ViewParent;

    move-result-object v0

    check-cast v0, Landroid/view/ViewGroup;

    invoke-static {v0}, La/a/a/c/KC_a;->a(Landroid/view/ViewGroup;)V

    new-instance v0, Lcom/kamcord/android/KC_P$KC_b;

    invoke-direct {v0, p0}, Lcom/kamcord/android/KC_P$KC_b;-><init>(Lcom/kamcord/android/KC_P;)V

    const/4 v3, 0x2

    new-array v3, v3, [Ljava/lang/String;

    aput-object p1, v3, v1

    aput-object p2, v3, v2

    invoke-virtual {v0, v3}, Lcom/kamcord/android/KC_P$KC_b;->execute([Ljava/lang/Object;)Landroid/os/AsyncTask;

    goto :goto_1

    :cond_3
    move v0, v2

    goto :goto_0
.end method
