.class final Lcom/kamcord/android/KC_N$KC_a;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/view/View$OnClickListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/KC_N;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = "KC_a"
.end annotation


# instance fields
.field private a:Lcom/kamcord/android/core/KC_H;

.field private b:Lcom/kamcord/android/KC_N;

.field private synthetic c:Lcom/kamcord/android/KC_N;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_N;Lcom/kamcord/android/KC_N;Lcom/kamcord/android/core/KC_H;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_N$KC_a;->c:Lcom/kamcord/android/KC_N;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    iput-object p2, p0, Lcom/kamcord/android/KC_N$KC_a;->b:Lcom/kamcord/android/KC_N;

    iput-object p3, p0, Lcom/kamcord/android/KC_N$KC_a;->a:Lcom/kamcord/android/core/KC_H;

    return-void
.end method


# virtual methods
.method public final onClick(Landroid/view/View;)V
    .locals 8

    const/4 v7, 0x4

    const/4 v1, 0x1

    const/4 v0, 0x0

    invoke-static {}, La/a/a/c/KC_a;->a()Z

    move-result v2

    if-nez v2, :cond_0

    new-instance v0, Lcom/kamcord/android/KC_x;

    invoke-direct {v0}, Lcom/kamcord/android/KC_x;-><init>()V

    iget-object v1, p0, Lcom/kamcord/android/KC_N$KC_a;->c:Lcom/kamcord/android/KC_N;

    invoke-virtual {v1}, Lcom/kamcord/android/KC_N;->h()La/a/a/a/KC_f;

    move-result-object v1

    invoke-virtual {v1}, La/a/a/a/KC_f;->getFragmentManager()Landroid/app/FragmentManager;

    move-result-object v1

    const/4 v2, 0x0

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/android/KC_x;->show(Landroid/app/FragmentManager;Ljava/lang/String;)V

    :goto_0
    return-void

    :cond_0
    iget-object v2, p0, Lcom/kamcord/android/KC_N$KC_a;->b:Lcom/kamcord/android/KC_N;

    iget-object v2, v2, Lcom/kamcord/android/KC_N;->P:Landroid/widget/EditText;

    invoke-virtual {v2, v0}, Landroid/widget/EditText;->setFocusable(Z)V

    iget-object v2, p0, Lcom/kamcord/android/KC_N$KC_a;->b:Lcom/kamcord/android/KC_N;

    iget-object v2, v2, Lcom/kamcord/android/KC_N;->P:Landroid/widget/EditText;

    invoke-virtual {v2, v0}, Landroid/widget/EditText;->setFocusableInTouchMode(Z)V

    iget-object v2, p0, Lcom/kamcord/android/KC_N$KC_a;->b:Lcom/kamcord/android/KC_N;

    iget-object v2, v2, Lcom/kamcord/android/KC_N;->P:Landroid/widget/EditText;

    invoke-virtual {v2, v0}, Landroid/widget/EditText;->setEnabled(Z)V

    iget-object v2, p0, Lcom/kamcord/android/KC_N$KC_a;->b:Lcom/kamcord/android/KC_N;

    iget-object v2, v2, Lcom/kamcord/android/KC_N;->O:Landroid/widget/Button;

    const-string v3, "string"

    const-string v4, "kamcordUploading"

    invoke-static {v3, v4}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v3

    invoke-virtual {v2, v3}, Landroid/widget/Button;->setText(I)V

    iget-object v2, p0, Lcom/kamcord/android/KC_N$KC_a;->b:Lcom/kamcord/android/KC_N;

    iget-object v2, v2, Lcom/kamcord/android/KC_N;->O:Landroid/widget/Button;

    invoke-virtual {v2, v0}, Landroid/widget/Button;->setEnabled(Z)V

    move v2, v0

    :goto_1
    if-ge v2, v7, :cond_1

    iget-object v3, p0, Lcom/kamcord/android/KC_N$KC_a;->b:Lcom/kamcord/android/KC_N;

    iget-object v3, v3, Lcom/kamcord/android/KC_N;->S:[Landroid/widget/LinearLayout;

    aget-object v3, v3, v2

    invoke-virtual {v3, v0}, Landroid/widget/LinearLayout;->setEnabled(Z)V

    add-int/lit8 v2, v2, 0x1

    goto :goto_1

    :cond_1
    new-instance v3, Lcom/kamcord/android/KC_W;

    invoke-direct {v3}, Lcom/kamcord/android/KC_W;-><init>()V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v4

    move v2, v0

    :goto_2
    if-ge v2, v7, :cond_3

    iget-object v5, p0, Lcom/kamcord/android/KC_N$KC_a;->c:Lcom/kamcord/android/KC_N;

    iget-object v5, v5, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    aget-object v5, v5, v2

    if-eqz v5, :cond_2

    new-instance v5, Ljava/lang/StringBuilder;

    const-string v6, "shareOn"

    invoke-direct {v5, v6}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v6, p0, Lcom/kamcord/android/KC_N$KC_a;->c:Lcom/kamcord/android/KC_N;

    iget-object v6, v6, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    aget-object v6, v6, v2

    invoke-virtual {v6}, Lcom/kamcord/android/b/KC_e;->d()Ljava/lang/String;

    move-result-object v6

    invoke-virtual {v5, v6}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v5

    invoke-virtual {v5}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v5

    invoke-interface {v4, v5, v0}, Landroid/content/SharedPreferences;->getBoolean(Ljava/lang/String;Z)Z

    move-result v5

    if-eqz v5, :cond_2

    iget-object v5, p0, Lcom/kamcord/android/KC_N$KC_a;->c:Lcom/kamcord/android/KC_N;

    iget-object v5, v5, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    aget-object v5, v5, v2

    invoke-virtual {v5}, Lcom/kamcord/android/b/KC_e;->d()Ljava/lang/String;

    move-result-object v5

    sget-object v6, Ljava/util/Locale;->ENGLISH:Ljava/util/Locale;

    invoke-virtual {v5, v6}, Ljava/lang/String;->toLowerCase(Ljava/util/Locale;)Ljava/lang/String;

    move-result-object v5

    invoke-virtual {v3, v5}, Lcom/kamcord/android/KC_W;->b(Ljava/lang/String;)Lcom/kamcord/android/KC_W;

    :cond_2
    add-int/lit8 v2, v2, 0x1

    goto :goto_2

    :cond_3
    iget-object v2, p0, Lcom/kamcord/android/KC_N$KC_a;->b:Lcom/kamcord/android/KC_N;

    iget-object v2, v2, Lcom/kamcord/android/KC_N;->P:Landroid/widget/EditText;

    invoke-virtual {v2}, Landroid/widget/EditText;->getEditableText()Landroid/text/Editable;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/Object;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/String;->length()I

    move-result v4

    if-lez v4, :cond_4

    invoke-virtual {v3, v2}, Lcom/kamcord/android/KC_W;->a(Ljava/lang/String;)Lcom/kamcord/android/KC_W;

    :cond_4
    iget-object v2, p0, Lcom/kamcord/android/KC_N$KC_a;->a:Lcom/kamcord/android/core/KC_H;

    iget-wide v4, v2, Lcom/kamcord/android/core/KC_H;->h:D

    invoke-virtual {v3, v4, v5}, Lcom/kamcord/android/KC_W;->a(D)Lcom/kamcord/android/KC_W;

    iget-object v2, p0, Lcom/kamcord/android/KC_N$KC_a;->a:Lcom/kamcord/android/core/KC_H;

    iget-object v2, v2, Lcom/kamcord/android/core/KC_H;->b:Ljava/lang/String;

    invoke-virtual {v3, v2}, Lcom/kamcord/android/KC_W;->c(Ljava/lang/String;)Lcom/kamcord/android/KC_W;

    iget-object v2, p0, Lcom/kamcord/android/KC_N$KC_a;->a:Lcom/kamcord/android/core/KC_H;

    iget-object v2, v2, Lcom/kamcord/android/core/KC_H;->a:Ljava/lang/String;

    invoke-virtual {v3, v2}, Lcom/kamcord/android/KC_W;->d(Ljava/lang/String;)Lcom/kamcord/android/KC_W;

    iget-object v2, p0, Lcom/kamcord/android/KC_N$KC_a;->a:Lcom/kamcord/android/core/KC_H;

    invoke-virtual {v2}, Lcom/kamcord/android/core/KC_H;->b()Z

    move-result v2

    if-eqz v2, :cond_5

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v2

    const-string v4, "voice_overlay_enabled"

    invoke-interface {v2, v4, v0}, Landroid/content/SharedPreferences;->getBoolean(Ljava/lang/String;Z)Z

    move-result v2

    if-eqz v2, :cond_5

    iget-object v2, p0, Lcom/kamcord/android/KC_N$KC_a;->c:Lcom/kamcord/android/KC_N;

    iget-object v2, v2, Lcom/kamcord/android/KC_N;->R:Landroid/widget/ToggleButton;

    invoke-virtual {v2}, Landroid/widget/ToggleButton;->isChecked()Z

    move-result v2

    if-eqz v2, :cond_5

    move v0, v1

    :cond_5
    invoke-virtual {v3, v0}, Lcom/kamcord/android/KC_W;->a(Z)Lcom/kamcord/android/KC_W;

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KC_N$KC_a;->a:Lcom/kamcord/android/core/KC_H;

    const/16 v2, 0x3e8

    const-string v4, "KamcordInfoFingerprint"

    sget-object v5, Landroid/os/Build;->FINGERPRINT:Ljava/lang/String;

    invoke-virtual {v0, v2, v4, v5}, Lcom/kamcord/android/core/KC_H;->a(ILjava/lang/String;Ljava/lang/String;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N$KC_a;->a:Lcom/kamcord/android/core/KC_H;

    const/16 v2, 0x3e8

    const-string v4, "KamcordInfoDevice"

    sget-object v5, Landroid/os/Build;->DEVICE:Ljava/lang/String;

    invoke-virtual {v0, v2, v4, v5}, Lcom/kamcord/android/core/KC_H;->a(ILjava/lang/String;Ljava/lang/String;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N$KC_a;->a:Lcom/kamcord/android/core/KC_H;

    const/16 v2, 0x3e8

    const-string v4, "KamcordInfoBoard"

    sget-object v5, Landroid/os/Build;->BOARD:Ljava/lang/String;

    invoke-virtual {v0, v2, v4, v5}, Lcom/kamcord/android/core/KC_H;->a(ILjava/lang/String;Ljava/lang/String;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N$KC_a;->a:Lcom/kamcord/android/core/KC_H;

    const/16 v2, 0x3e8

    const-string v4, "KamcordInfoID"

    sget-object v5, Landroid/os/Build;->ID:Ljava/lang/String;

    invoke-virtual {v0, v2, v4, v5}, Lcom/kamcord/android/core/KC_H;->a(ILjava/lang/String;Ljava/lang/String;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N$KC_a;->a:Lcom/kamcord/android/core/KC_H;

    const/16 v2, 0x3e8

    const-string v4, "KamcordInfoVersion"

    new-instance v5, Ljava/lang/StringBuilder;

    invoke-direct {v5}, Ljava/lang/StringBuilder;-><init>()V

    sget v6, Landroid/os/Build$VERSION;->SDK_INT:I

    invoke-virtual {v5, v6}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v5

    invoke-virtual {v5}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v5

    invoke-virtual {v0, v2, v4, v5}, Lcom/kamcord/android/core/KC_H;->a(ILjava/lang/String;Ljava/lang/String;)V
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    :goto_3
    iget-object v0, p0, Lcom/kamcord/android/KC_N$KC_a;->a:Lcom/kamcord/android/core/KC_H;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_H;->g:Lorg/json/JSONObject;

    invoke-virtual {v3, v0}, Lcom/kamcord/android/KC_W;->a(Lorg/json/JSONObject;)Lcom/kamcord/android/KC_W;

    iget-object v0, p0, Lcom/kamcord/android/KC_N$KC_a;->a:Lcom/kamcord/android/core/KC_H;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_H;->i:Ljava/lang/String;

    invoke-virtual {v3, v0}, Lcom/kamcord/android/KC_W;->e(Ljava/lang/String;)Lcom/kamcord/android/KC_W;

    iget-object v0, p0, Lcom/kamcord/android/KC_N$KC_a;->a:Lcom/kamcord/android/core/KC_H;

    iget-boolean v0, v0, Lcom/kamcord/android/core/KC_H;->e:Z

    if-eqz v0, :cond_6

    iget-object v0, p0, Lcom/kamcord/android/KC_N$KC_a;->a:Lcom/kamcord/android/core/KC_H;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_H;->i:Ljava/lang/String;

    if-eqz v0, :cond_6

    invoke-virtual {v3, v1}, Lcom/kamcord/android/KC_W;->b(Z)Lcom/kamcord/android/KC_W;

    :goto_4
    iget-object v0, p0, Lcom/kamcord/android/KC_N$KC_a;->c:Lcom/kamcord/android/KC_N;

    invoke-static {v0}, Lcom/kamcord/android/KC_Y;->a(Lcom/kamcord/android/KC_X;)V

    invoke-virtual {v3}, Lcom/kamcord/android/KC_W;->a()Landroid/content/Intent;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/android/KC_N$KC_a;->b:Lcom/kamcord/android/KC_N;

    invoke-virtual {v1}, Lcom/kamcord/android/KC_N;->h()La/a/a/a/KC_f;

    move-result-object v1

    invoke-virtual {v1, v0}, La/a/a/a/KC_f;->startService(Landroid/content/Intent;)Landroid/content/ComponentName;

    goto/16 :goto_0

    :catch_0
    move-exception v0

    const-string v2, "Unable to attach Kamcord metadata to video!"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    invoke-virtual {v0}, Lorg/json/JSONException;->printStackTrace()V

    goto :goto_3

    :cond_6
    iget-object v0, p0, Lcom/kamcord/android/KC_N$KC_a;->c:Lcom/kamcord/android/KC_N;

    iget-object v0, v0, Lcom/kamcord/android/KC_N;->M:Landroid/app/Activity;

    const-string v1, "id"

    const-string v2, "mainlayout"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/app/Activity;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/view/ViewGroup;

    invoke-static {v0}, La/a/a/c/KC_a;->a(Landroid/view/ViewGroup;)V

    goto :goto_4
.end method
