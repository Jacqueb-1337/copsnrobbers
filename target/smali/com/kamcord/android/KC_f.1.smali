.class public final Lcom/kamcord/android/KC_f;
.super La/a/a/a/KC_e;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/KC_f$3;,
        Lcom/kamcord/android/KC_f$KC_b;,
        Lcom/kamcord/android/KC_f$KC_a;
    }
.end annotation


# instance fields
.field M:Lcom/kamcord/android/KamcordActivity;

.field N:Lcom/kamcord/android/KC_f$KC_a;

.field private O:Landroid/view/View;

.field private P:Landroid/widget/EditText;

.field private Q:Landroid/widget/EditText;

.field private R:Landroid/widget/EditText;

.field private S:Landroid/widget/Button;


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, La/a/a/a/KC_e;-><init>()V

    return-void
.end method

.method static synthetic a(Lcom/kamcord/android/KC_f;)Landroid/widget/EditText;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_f;->Q:Landroid/widget/EditText;

    return-object v0
.end method

.method static synthetic b(Lcom/kamcord/android/KC_f;)Landroid/widget/EditText;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_f;->P:Landroid/widget/EditText;

    return-object v0
.end method


# virtual methods
.method public final B()V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/KC_f;->P:Landroid/widget/EditText;

    const-string v1, ""

    invoke-virtual {v0, v1}, Landroid/widget/EditText;->setText(Ljava/lang/CharSequence;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_f;->Q:Landroid/widget/EditText;

    const-string v1, ""

    invoke-virtual {v0, v1}, Landroid/widget/EditText;->setText(Ljava/lang/CharSequence;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_f;->R:Landroid/widget/EditText;

    const-string v1, ""

    invoke-virtual {v0, v1}, Landroid/widget/EditText;->setText(Ljava/lang/CharSequence;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_f;->M:Lcom/kamcord/android/KamcordActivity;

    const-string v1, "input_method"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KamcordActivity;->getSystemService(Ljava/lang/String;)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Landroid/view/inputmethod/InputMethodManager;

    iget-object v1, p0, Lcom/kamcord/android/KC_f;->O:Landroid/view/View;

    invoke-virtual {v1}, Landroid/view/View;->getWindowToken()Landroid/os/IBinder;

    move-result-object v1

    const/4 v2, 0x0

    invoke-virtual {v0, v1, v2}, Landroid/view/inputmethod/InputMethodManager;->hideSoftInputFromWindow(Landroid/os/IBinder;I)Z

    return-void
.end method

.method public final a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
    .locals 6

    const-string v0, "layout"

    const-string v1, "z_kamcord_fragment_change_creds"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    const/4 v1, 0x0

    invoke-virtual {p1, v0, p2, v1}, Landroid/view/LayoutInflater;->inflate(ILandroid/view/ViewGroup;Z)Landroid/view/View;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_f;->O:Landroid/view/View;

    invoke-virtual {p0}, Lcom/kamcord/android/KC_f;->g()Landroid/os/Bundle;

    move-result-object v0

    if-eqz v0, :cond_0

    const-string v1, "credType"

    invoke-virtual {v0, v1}, Landroid/os/Bundle;->containsKey(Ljava/lang/String;)Z

    move-result v1

    if-eqz v1, :cond_0

    const-string v1, "credType"

    invoke-virtual {v0, v1}, Landroid/os/Bundle;->getSerializable(Ljava/lang/String;)Ljava/io/Serializable;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_f$KC_a;

    iput-object v0, p0, Lcom/kamcord/android/KC_f;->N:Lcom/kamcord/android/KC_f$KC_a;

    :goto_0
    iget-object v0, p0, Lcom/kamcord/android/KC_f;->O:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "currentPasswordEditText"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/EditText;

    iput-object v0, p0, Lcom/kamcord/android/KC_f;->P:Landroid/widget/EditText;

    iget-object v0, p0, Lcom/kamcord/android/KC_f;->O:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "newCredEditText"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/EditText;

    iput-object v0, p0, Lcom/kamcord/android/KC_f;->Q:Landroid/widget/EditText;

    iget-object v0, p0, Lcom/kamcord/android/KC_f;->O:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "newCredAgainEditText"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/EditText;

    iput-object v0, p0, Lcom/kamcord/android/KC_f;->R:Landroid/widget/EditText;

    iget-object v0, p0, Lcom/kamcord/android/KC_f;->O:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "changeCredButton"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/Button;

    iput-object v0, p0, Lcom/kamcord/android/KC_f;->S:Landroid/widget/Button;

    const-string v3, ""

    const-string v2, ""

    const/4 v1, -0x1

    const-string v0, ""

    sget-object v4, Lcom/kamcord/android/KC_f$3;->a:[I

    iget-object v5, p0, Lcom/kamcord/android/KC_f;->N:Lcom/kamcord/android/KC_f$KC_a;

    invoke-virtual {v5}, Lcom/kamcord/android/KC_f$KC_a;->ordinal()I

    move-result v5

    aget v4, v4, v5

    packed-switch v4, :pswitch_data_0

    :goto_1
    iget-object v4, p0, Lcom/kamcord/android/KC_f;->Q:Landroid/widget/EditText;

    invoke-virtual {v4, v3}, Landroid/widget/EditText;->setHint(Ljava/lang/CharSequence;)V

    iget-object v3, p0, Lcom/kamcord/android/KC_f;->Q:Landroid/widget/EditText;

    invoke-virtual {v3, v1}, Landroid/widget/EditText;->setInputType(I)V

    iget-object v3, p0, Lcom/kamcord/android/KC_f;->R:Landroid/widget/EditText;

    invoke-virtual {v3, v2}, Landroid/widget/EditText;->setHint(Ljava/lang/CharSequence;)V

    iget-object v2, p0, Lcom/kamcord/android/KC_f;->R:Landroid/widget/EditText;

    invoke-virtual {v2, v1}, Landroid/widget/EditText;->setInputType(I)V

    iget-object v1, p0, Lcom/kamcord/android/KC_f;->S:Landroid/widget/Button;

    invoke-virtual {v1, v0}, Landroid/widget/Button;->setText(Ljava/lang/CharSequence;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_f;->O:Landroid/view/View;

    :goto_2
    iput-object v0, p0, Lcom/kamcord/android/KC_f;->O:Landroid/view/View;

    iget-object v0, p0, Lcom/kamcord/android/KC_f;->R:Landroid/widget/EditText;

    new-instance v1, Lcom/kamcord/android/KC_f$1;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_f$1;-><init>(Lcom/kamcord/android/KC_f;)V

    invoke-virtual {v0, v1}, Landroid/widget/EditText;->setOnEditorActionListener(Landroid/widget/TextView$OnEditorActionListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_f;->S:Landroid/widget/Button;

    new-instance v1, Lcom/kamcord/android/KC_f$2;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_f$2;-><init>(Lcom/kamcord/android/KC_f;)V

    invoke-virtual {v0, v1}, Landroid/widget/Button;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_f;->O:Landroid/view/View;

    return-object v0

    :cond_0
    if-eqz p3, :cond_1

    const-string v0, "credType"

    invoke-virtual {p3, v0}, Landroid/os/Bundle;->containsKey(Ljava/lang/String;)Z

    move-result v0

    if-eqz v0, :cond_1

    const-string v0, "credType"

    invoke-virtual {p3, v0}, Landroid/os/Bundle;->getSerializable(Ljava/lang/String;)Ljava/io/Serializable;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_f$KC_a;

    iput-object v0, p0, Lcom/kamcord/android/KC_f;->N:Lcom/kamcord/android/KC_f$KC_a;

    goto/16 :goto_0

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_f;->O:Landroid/view/View;

    goto :goto_2

    :pswitch_0
    const-string v0, "kamcordNewEmail"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    const-string v0, "kamcordNewEmailAgain"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    const/16 v1, 0x21

    const-string v0, "kamcordChangeEmail"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    goto :goto_1

    :pswitch_1
    const-string v0, "kamcordNewPassword"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    const-string v0, "kamcordNewPasswordAgain"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    const/16 v1, 0x81

    const-string v0, "kamcordChangePassword"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    goto :goto_1

    :pswitch_data_0
    .packed-switch 0x1
        :pswitch_0
        :pswitch_1
    .end packed-switch
.end method

.method public final a(Landroid/app/Activity;)V
    .locals 0

    invoke-super {p0, p1}, La/a/a/a/KC_e;->a(Landroid/app/Activity;)V

    check-cast p1, Lcom/kamcord/android/KamcordActivity;

    iput-object p1, p0, Lcom/kamcord/android/KC_f;->M:Lcom/kamcord/android/KamcordActivity;

    return-void
.end method

.method public final b()V
    .locals 8

    const/4 v2, 0x1

    const/4 v1, 0x0

    iget-object v0, p0, Lcom/kamcord/android/KC_f;->P:Landroid/widget/EditText;

    invoke-virtual {v0}, Landroid/widget/EditText;->getText()Landroid/text/Editable;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/Object;->toString()Ljava/lang/String;

    move-result-object v3

    iget-object v0, p0, Lcom/kamcord/android/KC_f;->Q:Landroid/widget/EditText;

    invoke-virtual {v0}, Landroid/widget/EditText;->getText()Landroid/text/Editable;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/Object;->toString()Ljava/lang/String;

    move-result-object v4

    iget-object v0, p0, Lcom/kamcord/android/KC_f;->R:Landroid/widget/EditText;

    invoke-virtual {v0}, Landroid/widget/EditText;->getText()Landroid/text/Editable;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/Object;->toString()Ljava/lang/String;

    move-result-object v5

    invoke-virtual {v3}, Ljava/lang/String;->length()I

    move-result v0

    if-nez v0, :cond_6

    iget-object v0, p0, Lcom/kamcord/android/KC_f;->P:Landroid/widget/EditText;

    const-string v6, "kamcordPasswordEnter"

    invoke-static {v6}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v6

    invoke-virtual {v0, v6}, Landroid/widget/EditText;->setError(Ljava/lang/CharSequence;)V

    move v0, v1

    :goto_0
    invoke-virtual {v4}, Ljava/lang/String;->length()I

    move-result v6

    if-nez v6, :cond_0

    sget-object v0, Lcom/kamcord/android/KC_f$3;->a:[I

    iget-object v6, p0, Lcom/kamcord/android/KC_f;->N:Lcom/kamcord/android/KC_f$KC_a;

    invoke-virtual {v6}, Lcom/kamcord/android/KC_f$KC_a;->ordinal()I

    move-result v6

    aget v0, v0, v6

    packed-switch v0, :pswitch_data_0

    :goto_1
    move v0, v1

    :cond_0
    invoke-virtual {v5}, Ljava/lang/String;->length()I

    move-result v6

    if-nez v6, :cond_1

    sget-object v0, Lcom/kamcord/android/KC_f$3;->a:[I

    iget-object v6, p0, Lcom/kamcord/android/KC_f;->N:Lcom/kamcord/android/KC_f$KC_a;

    invoke-virtual {v6}, Lcom/kamcord/android/KC_f$KC_a;->ordinal()I

    move-result v6

    aget v0, v0, v6

    packed-switch v0, :pswitch_data_1

    :goto_2
    move v0, v1

    :cond_1
    iget-object v6, p0, Lcom/kamcord/android/KC_f;->N:Lcom/kamcord/android/KC_f$KC_a;

    sget-object v7, Lcom/kamcord/android/KC_f$KC_a;->a:Lcom/kamcord/android/KC_f$KC_a;

    if-ne v6, v7, :cond_2

    sget-object v6, Landroid/util/Patterns;->EMAIL_ADDRESS:Ljava/util/regex/Pattern;

    invoke-virtual {v6, v4}, Ljava/util/regex/Pattern;->matcher(Ljava/lang/CharSequence;)Ljava/util/regex/Matcher;

    move-result-object v6

    invoke-virtual {v6}, Ljava/util/regex/Matcher;->matches()Z

    move-result v6

    if-nez v6, :cond_2

    iget-object v0, p0, Lcom/kamcord/android/KC_f;->Q:Landroid/widget/EditText;

    const-string v6, "kamcordEmailInvalid"

    invoke-static {v6}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v6

    invoke-virtual {v0, v6}, Landroid/widget/EditText;->setError(Ljava/lang/CharSequence;)V

    move v0, v1

    :cond_2
    invoke-virtual {v4, v5}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v5

    if-nez v5, :cond_3

    const-string v0, ""

    sget-object v5, Lcom/kamcord/android/KC_f$3;->a:[I

    iget-object v6, p0, Lcom/kamcord/android/KC_f;->N:Lcom/kamcord/android/KC_f$KC_a;

    invoke-virtual {v6}, Lcom/kamcord/android/KC_f$KC_a;->ordinal()I

    move-result v6

    aget v5, v5, v6

    packed-switch v5, :pswitch_data_2

    :goto_3
    iget-object v5, p0, Lcom/kamcord/android/KC_f;->R:Landroid/widget/EditText;

    invoke-virtual {v5, v0}, Landroid/widget/EditText;->setError(Ljava/lang/CharSequence;)V

    move v0, v1

    :cond_3
    if-nez v0, :cond_4

    :goto_4
    return-void

    :pswitch_0
    iget-object v0, p0, Lcom/kamcord/android/KC_f;->Q:Landroid/widget/EditText;

    const-string v6, "kamcordEmailEnter"

    invoke-static {v6}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v6

    invoke-virtual {v0, v6}, Landroid/widget/EditText;->setError(Ljava/lang/CharSequence;)V

    goto :goto_1

    :pswitch_1
    iget-object v0, p0, Lcom/kamcord/android/KC_f;->Q:Landroid/widget/EditText;

    const-string v6, "kamcordPasswordEnter"

    invoke-static {v6}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v6

    invoke-virtual {v0, v6}, Landroid/widget/EditText;->setError(Ljava/lang/CharSequence;)V

    goto :goto_1

    :pswitch_2
    iget-object v0, p0, Lcom/kamcord/android/KC_f;->R:Landroid/widget/EditText;

    const-string v6, "kamcordEmailEnter"

    invoke-static {v6}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v6

    invoke-virtual {v0, v6}, Landroid/widget/EditText;->setError(Ljava/lang/CharSequence;)V

    goto :goto_2

    :pswitch_3
    iget-object v0, p0, Lcom/kamcord/android/KC_f;->R:Landroid/widget/EditText;

    const-string v6, "kamcordPasswordEnter"

    invoke-static {v6}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v6

    invoke-virtual {v0, v6}, Landroid/widget/EditText;->setError(Ljava/lang/CharSequence;)V

    goto :goto_2

    :pswitch_4
    const-string v0, "kamcordNewEmailsDoNotMatch"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    goto :goto_3

    :pswitch_5
    const-string v0, "kamcordNewPasswordsDoNotMatch"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    goto :goto_3

    :cond_4
    invoke-static {}, La/a/a/c/KC_a;->a()Z

    move-result v0

    if-nez v0, :cond_5

    new-instance v0, Lcom/kamcord/android/KC_x;

    invoke-direct {v0}, Lcom/kamcord/android/KC_x;-><init>()V

    invoke-virtual {p0}, Lcom/kamcord/android/KC_f;->h()La/a/a/a/KC_f;

    move-result-object v1

    invoke-virtual {v1}, La/a/a/a/KC_f;->getFragmentManager()Landroid/app/FragmentManager;

    move-result-object v1

    const/4 v2, 0x0

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/android/KC_x;->show(Landroid/app/FragmentManager;Ljava/lang/String;)V

    goto :goto_4

    :cond_5
    new-instance v0, Lcom/kamcord/android/KC_f$KC_b;

    invoke-direct {v0, p0}, Lcom/kamcord/android/KC_f$KC_b;-><init>(Lcom/kamcord/android/KC_f;)V

    const/4 v5, 0x2

    new-array v5, v5, [Ljava/lang/String;

    aput-object v4, v5, v1

    aput-object v3, v5, v2

    invoke-virtual {v0, v5}, Lcom/kamcord/android/KC_f$KC_b;->execute([Ljava/lang/Object;)Landroid/os/AsyncTask;

    goto :goto_4

    :cond_6
    move v0, v2

    goto/16 :goto_0

    nop

    :pswitch_data_0
    .packed-switch 0x1
        :pswitch_0
        :pswitch_1
    .end packed-switch

    :pswitch_data_1
    .packed-switch 0x1
        :pswitch_2
        :pswitch_3
    .end packed-switch

    :pswitch_data_2
    .packed-switch 0x1
        :pswitch_4
        :pswitch_5
    .end packed-switch
.end method
