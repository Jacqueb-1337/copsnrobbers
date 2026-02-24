.class public final Lcom/kamcord/android/KC_h;
.super La/a/a/a/KC_e;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/KC_h$KC_b;,
        Lcom/kamcord/android/KC_h$KC_a;,
        Lcom/kamcord/android/KC_h$KC_c;
    }
.end annotation


# instance fields
.field private M:Lcom/kamcord/android/KamcordActivity;

.field private N:Landroid/view/View;

.field private O:Landroid/widget/EditText;

.field private P:Landroid/widget/EditText;

.field private Q:Landroid/widget/EditText;

.field private R:Landroid/widget/Button;


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, La/a/a/a/KC_e;-><init>()V

    return-void
.end method

.method static synthetic a(Lcom/kamcord/android/KC_h;)Landroid/widget/EditText;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->O:Landroid/widget/EditText;

    return-object v0
.end method

.method static synthetic a(Lcom/kamcord/android/KC_h;Z)V
    .locals 1

    const/4 v0, 0x1

    invoke-direct {p0, v0}, Lcom/kamcord/android/KC_h;->c(Z)V

    return-void
.end method

.method static synthetic b(Lcom/kamcord/android/KC_h;)Landroid/widget/EditText;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->P:Landroid/widget/EditText;

    return-object v0
.end method

.method static synthetic c(Lcom/kamcord/android/KC_h;)Landroid/widget/EditText;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->Q:Landroid/widget/EditText;

    return-object v0
.end method

.method private c(Z)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->R:Landroid/widget/Button;

    invoke-virtual {v0, p1}, Landroid/widget/Button;->setEnabled(Z)V

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->O:Landroid/widget/EditText;

    invoke-virtual {v0, p1}, Landroid/widget/EditText;->setEnabled(Z)V

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->P:Landroid/widget/EditText;

    invoke-virtual {v0, p1}, Landroid/widget/EditText;->setEnabled(Z)V

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->Q:Landroid/widget/EditText;

    invoke-virtual {v0, p1}, Landroid/widget/EditText;->setEnabled(Z)V

    return-void
.end method

.method static synthetic d(Lcom/kamcord/android/KC_h;)Lcom/kamcord/android/KamcordActivity;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->M:Lcom/kamcord/android/KamcordActivity;

    return-object v0
.end method

.method static synthetic e(Lcom/kamcord/android/KC_h;)Landroid/view/View;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->N:Landroid/view/View;

    return-object v0
.end method


# virtual methods
.method public final a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
    .locals 8

    const-string v0, "layout"

    const-string v1, "z_kamcord_fragment_create_profile"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    const/4 v1, 0x0

    invoke-virtual {p1, v0, p2, v1}, Landroid/view/LayoutInflater;->inflate(ILandroid/view/ViewGroup;Z)Landroid/view/View;

    move-result-object v1

    const-string v0, "id"

    const-string v2, "privacyAndTerms"

    invoke-static {v0, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {v1, v0}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/CustomWebView;

    new-instance v2, Lcom/kamcord/android/KC_h$KC_b;

    invoke-direct {v2, p0}, Lcom/kamcord/android/KC_h$KC_b;-><init>(Lcom/kamcord/android/KC_h;)V

    invoke-virtual {v0, v2}, Lcom/kamcord/android/CustomWebView;->setWebViewClient(Landroid/webkit/WebViewClient;)V

    const-string v2, "kamcordActivityBackground"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord;->getColor(Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v0, v2}, Lcom/kamcord/android/CustomWebView;->setBackgroundColor(I)V

    new-instance v2, Lcom/kamcord/android/KC_h$6;

    invoke-direct {v2, p0}, Lcom/kamcord/android/KC_h$6;-><init>(Lcom/kamcord/android/KC_h;)V

    invoke-virtual {v0, v2}, Lcom/kamcord/android/CustomWebView;->setOnTouchListener(Landroid/view/View$OnTouchListener;)V

    const-string v2, "kamcordAgreementBlurb"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    const-string v3, "kamcordPrivacyPolicy"

    invoke-static {v3}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    const-string v4, "kamcordTermsOfService"

    invoke-static {v4}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v4

    new-instance v5, Ljava/lang/StringBuilder;

    const-string v6, "<a style=\"color : #8c8f91;\" href=\"kc-link://{url:\'https://www.kamcord.com/privacy/\'}\">"

    invoke-direct {v5, v6}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v5, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v5

    const-string v6, "</a>"

    invoke-virtual {v5, v6}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v5

    invoke-virtual {v5}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v5

    new-instance v6, Ljava/lang/StringBuilder;

    const-string v7, "<a style=\"color : #8c8f91;\" href=\"kc-link://{url:\'https://www.kamcord.com/tos/\'}\">"

    invoke-direct {v6, v7}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v6, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v6

    const-string v7, "</a>"

    invoke-virtual {v6, v7}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v6

    invoke-virtual {v6}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v6

    const-string v7, "background-color : transparent;color: #8c8f91;font-family: \'Helvetica Neue\', sans-serif;text-align: center;font-weight : light;font-size : 12px;-webkit-touch-callout: none;-webkit-user-select: none;"

    invoke-virtual {v2, v3, v5}, Ljava/lang/String;->replace(Ljava/lang/CharSequence;Ljava/lang/CharSequence;)Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v2, v4, v6}, Ljava/lang/String;->replace(Ljava/lang/CharSequence;Ljava/lang/CharSequence;)Ljava/lang/String;

    move-result-object v2

    new-instance v3, Ljava/lang/StringBuilder;

    const-string v4, "<html><body style=\""

    invoke-direct {v3, v4}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v3, v7}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    const-string v4, "\">"

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    invoke-virtual {v3, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "</body></html>"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    const-string v3, "text/html; charset=utf-8"

    const-string v4, "utf-8"

    invoke-virtual {v0, v2, v3, v4}, Lcom/kamcord/android/CustomWebView;->loadData(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V

    iput-object v1, p0, Lcom/kamcord/android/KC_h;->N:Landroid/view/View;

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->N:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "username"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/EditText;

    iput-object v0, p0, Lcom/kamcord/android/KC_h;->O:Landroid/widget/EditText;

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->O:Landroid/widget/EditText;

    new-instance v1, Lcom/kamcord/android/KC_h$1;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_h$1;-><init>(Lcom/kamcord/android/KC_h;)V

    invoke-virtual {v0, v1}, Landroid/widget/EditText;->setOnFocusChangeListener(Landroid/view/View$OnFocusChangeListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->N:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "password"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/EditText;

    iput-object v0, p0, Lcom/kamcord/android/KC_h;->P:Landroid/widget/EditText;

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->P:Landroid/widget/EditText;

    new-instance v1, Lcom/kamcord/android/KC_h$2;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_h$2;-><init>(Lcom/kamcord/android/KC_h;)V

    invoke-virtual {v0, v1}, Landroid/widget/EditText;->setOnFocusChangeListener(Landroid/view/View$OnFocusChangeListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->N:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "email"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/EditText;

    iput-object v0, p0, Lcom/kamcord/android/KC_h;->Q:Landroid/widget/EditText;

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->Q:Landroid/widget/EditText;

    new-instance v1, Lcom/kamcord/android/KC_h$3;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_h$3;-><init>(Lcom/kamcord/android/KC_h;)V

    invoke-virtual {v0, v1}, Landroid/widget/EditText;->setOnFocusChangeListener(Landroid/view/View$OnFocusChangeListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->Q:Landroid/widget/EditText;

    new-instance v1, Lcom/kamcord/android/KC_h$4;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_h$4;-><init>(Lcom/kamcord/android/KC_h;)V

    invoke-virtual {v0, v1}, Landroid/widget/EditText;->setOnEditorActionListener(Landroid/widget/TextView$OnEditorActionListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->N:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "createProfile"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/Button;

    iput-object v0, p0, Lcom/kamcord/android/KC_h;->R:Landroid/widget/Button;

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->R:Landroid/widget/Button;

    new-instance v1, Lcom/kamcord/android/KC_h$5;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_h$5;-><init>(Lcom/kamcord/android/KC_h;)V

    invoke-virtual {v0, v1}, Landroid/widget/Button;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->N:Landroid/view/View;

    return-object v0
.end method

.method public final a(Landroid/app/Activity;)V
    .locals 0

    invoke-super {p0, p1}, La/a/a/a/KC_e;->a(Landroid/app/Activity;)V

    check-cast p1, Lcom/kamcord/android/KamcordActivity;

    iput-object p1, p0, Lcom/kamcord/android/KC_h;->M:Lcom/kamcord/android/KamcordActivity;

    return-void
.end method

.method final a(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V
    .locals 5

    const/4 v1, 0x0

    invoke-direct {p0, v1}, Lcom/kamcord/android/KC_h;->c(Z)V

    const/4 v0, 0x1

    invoke-virtual {p0, p1}, Lcom/kamcord/android/KC_h;->a(Ljava/lang/String;)Z

    move-result v2

    if-nez v2, :cond_0

    move v0, v1

    :cond_0
    invoke-virtual {p0, p2}, Lcom/kamcord/android/KC_h;->b(Ljava/lang/String;)Z

    move-result v2

    if-nez v2, :cond_1

    move v0, v1

    :cond_1
    invoke-virtual {p0, p3}, Lcom/kamcord/android/KC_h;->c(Ljava/lang/String;)Z

    move-result v2

    if-nez v2, :cond_2

    move v0, v1

    :cond_2
    if-eqz v0, :cond_5

    invoke-virtual {p1}, Ljava/lang/String;->length()I

    move-result v2

    if-nez v2, :cond_3

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->O:Landroid/widget/EditText;

    const-string v2, "kamcordUsernameEnter"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v2}, Landroid/widget/EditText;->setError(Ljava/lang/CharSequence;)V

    move v0, v1

    :cond_3
    invoke-virtual {p2}, Ljava/lang/String;->length()I

    move-result v2

    if-nez v2, :cond_4

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->P:Landroid/widget/EditText;

    const-string v2, "kamcordPasswordEnter"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v2}, Landroid/widget/EditText;->setError(Ljava/lang/CharSequence;)V

    move v0, v1

    :cond_4
    invoke-virtual {p3}, Ljava/lang/String;->length()I

    move-result v2

    if-nez v2, :cond_5

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->Q:Landroid/widget/EditText;

    const-string v2, "kamcordEmailEnter"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v2}, Landroid/widget/EditText;->setError(Ljava/lang/CharSequence;)V

    move v0, v1

    :cond_5
    if-nez v0, :cond_6

    :goto_0
    return-void

    :cond_6
    const-string v0, "kamcordEmailEntered"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    const-string v1, "%@"

    invoke-virtual {v0, v1, p3}, Ljava/lang/String;->replace(Ljava/lang/CharSequence;Ljava/lang/CharSequence;)Ljava/lang/String;

    move-result-object v0

    new-instance v1, Landroid/app/AlertDialog$Builder;

    invoke-virtual {p0}, Lcom/kamcord/android/KC_h;->h()La/a/a/a/KC_f;

    move-result-object v2

    invoke-direct {v1, v2}, Landroid/app/AlertDialog$Builder;-><init>(Landroid/content/Context;)V

    const-string v2, "kamcordYes"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    new-instance v3, Lcom/kamcord/android/KC_h$8;

    invoke-direct {v3, p0, p1, p2, p3}, Lcom/kamcord/android/KC_h$8;-><init>(Lcom/kamcord/android/KC_h;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V

    invoke-virtual {v1, v2, v3}, Landroid/app/AlertDialog$Builder;->setPositiveButton(Ljava/lang/CharSequence;Landroid/content/DialogInterface$OnClickListener;)Landroid/app/AlertDialog$Builder;

    move-result-object v2

    const-string v3, "kamcordNo"

    invoke-static {v3}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    const/4 v4, 0x0

    invoke-virtual {v2, v3, v4}, Landroid/app/AlertDialog$Builder;->setNegativeButton(Ljava/lang/CharSequence;Landroid/content/DialogInterface$OnClickListener;)Landroid/app/AlertDialog$Builder;

    move-result-object v2

    const-string v3, "kamcordEmailCorrect"

    invoke-static {v3}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Landroid/app/AlertDialog$Builder;->setTitle(Ljava/lang/CharSequence;)Landroid/app/AlertDialog$Builder;

    move-result-object v2

    invoke-virtual {v2, v0}, Landroid/app/AlertDialog$Builder;->setMessage(Ljava/lang/CharSequence;)Landroid/app/AlertDialog$Builder;

    move-result-object v0

    new-instance v2, Lcom/kamcord/android/KC_h$7;

    invoke-direct {v2, p0}, Lcom/kamcord/android/KC_h$7;-><init>(Lcom/kamcord/android/KC_h;)V

    invoke-virtual {v0, v2}, Landroid/app/AlertDialog$Builder;->setOnDismissListener(Landroid/content/DialogInterface$OnDismissListener;)Landroid/app/AlertDialog$Builder;

    invoke-virtual {v1}, Landroid/app/AlertDialog$Builder;->show()Landroid/app/AlertDialog;

    goto :goto_0
.end method

.method final a(Ljava/lang/String;)Z
    .locals 3

    const/4 v0, 0x0

    invoke-virtual {p1}, Ljava/lang/String;->length()I

    move-result v1

    const/4 v2, 0x5

    if-ge v1, v2, :cond_0

    invoke-virtual {p1}, Ljava/lang/String;->length()I

    move-result v1

    if-lez v1, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/KC_h;->O:Landroid/widget/EditText;

    const-string v2, "kamcordUsernameMinCharacters"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v1, v2}, Landroid/widget/EditText;->setError(Ljava/lang/CharSequence;)V

    :goto_0
    return v0

    :cond_0
    invoke-virtual {p1}, Ljava/lang/String;->length()I

    move-result v1

    const/16 v2, 0x10

    if-lt v1, v2, :cond_1

    iget-object v1, p0, Lcom/kamcord/android/KC_h;->O:Landroid/widget/EditText;

    const-string v2, "kamcordUsernameMaxCharacters"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v1, v2}, Landroid/widget/EditText;->setError(Ljava/lang/CharSequence;)V

    goto :goto_0

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_h;->O:Landroid/widget/EditText;

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Landroid/widget/EditText;->setError(Ljava/lang/CharSequence;)V

    const/4 v0, 0x1

    goto :goto_0
.end method

.method final b(Ljava/lang/String;)Z
    .locals 3

    const/4 v0, 0x0

    invoke-virtual {p1}, Ljava/lang/String;->length()I

    move-result v1

    const/4 v2, 0x6

    if-ge v1, v2, :cond_0

    invoke-virtual {p1}, Ljava/lang/String;->length()I

    move-result v1

    if-lez v1, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/KC_h;->P:Landroid/widget/EditText;

    const-string v2, "kamcordPasswordMinCharacters"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v1, v2}, Landroid/widget/EditText;->setError(Ljava/lang/CharSequence;)V

    :goto_0
    return v0

    :cond_0
    invoke-virtual {p1}, Ljava/lang/String;->length()I

    move-result v1

    const/16 v2, 0x1f

    if-lt v1, v2, :cond_1

    iget-object v1, p0, Lcom/kamcord/android/KC_h;->P:Landroid/widget/EditText;

    const-string v2, "kamcordPasswordMaxCharacters"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v1, v2}, Landroid/widget/EditText;->setError(Ljava/lang/CharSequence;)V

    goto :goto_0

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_h;->P:Landroid/widget/EditText;

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Landroid/widget/EditText;->setError(Ljava/lang/CharSequence;)V

    const/4 v0, 0x1

    goto :goto_0
.end method

.method final c(Ljava/lang/String;)Z
    .locals 2

    invoke-virtual {p1}, Ljava/lang/String;->length()I

    move-result v0

    if-lez v0, :cond_0

    sget-object v0, Landroid/util/Patterns;->EMAIL_ADDRESS:Ljava/util/regex/Pattern;

    invoke-virtual {v0, p1}, Ljava/util/regex/Pattern;->matcher(Ljava/lang/CharSequence;)Ljava/util/regex/Matcher;

    move-result-object v0

    invoke-virtual {v0}, Ljava/util/regex/Matcher;->matches()Z

    move-result v0

    if-nez v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_h;->Q:Landroid/widget/EditText;

    const-string v1, "kamcordEmailInvalid"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Landroid/widget/EditText;->setError(Ljava/lang/CharSequence;)V

    const/4 v0, 0x0

    :goto_0
    return v0

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_h;->Q:Landroid/widget/EditText;

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Landroid/widget/EditText;->setError(Ljava/lang/CharSequence;)V

    const/4 v0, 0x1

    goto :goto_0
.end method
