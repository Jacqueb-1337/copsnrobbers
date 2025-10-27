.class public Lcom/kamcord/android/KamcordActivity;
.super La/a/a/a/KC_f;

# interfaces
.implements Lcom/kamcord/android/KC_r;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/KamcordActivity$2;,
        Lcom/kamcord/android/KamcordActivity$KC_a;
    }
.end annotation


# static fields
.field public static final DEFAULT_MODE:I = 0x0

.field public static final WATCH_ONLY_MODE:I = 0x1


# instance fields
.field private e:Lcom/kamcord/android/KamcordActivity$KC_a;

.field private f:Lcom/kamcord/android/CustomViewPager;

.field private g:Lcom/kamcord/android/KC_T;

.field private h:Lcom/kamcord/android/KC_q;

.field private i:I

.field private j:I

.field private k:I

.field private l:I


# direct methods
.method public constructor <init>()V
    .locals 1

    invoke-direct {p0}, La/a/a/a/KC_f;-><init>()V

    const/4 v0, 0x0

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->k:I

    return-void
.end method


# virtual methods
.method public getTabBar()Lcom/kamcord/android/KC_q;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KamcordActivity;->h:Lcom/kamcord/android/KC_q;

    return-object v0
.end method

.method public getTabFragmentManager()Lcom/kamcord/android/KC_T;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KamcordActivity;->g:Lcom/kamcord/android/KC_T;

    return-object v0
.end method

.method public indexOfTabType(Lcom/kamcord/android/KC_p;)I
    .locals 4

    const/4 v1, -0x1

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KamcordActivity;->h:Lcom/kamcord/android/KC_q;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_q;->iterator()Ljava/util/Iterator;

    move-result-object v2

    :cond_0
    invoke-interface {v2}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_1

    invoke-interface {v2}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/c/KC_a;

    invoke-virtual {v0}, La/a/a/c/KC_a;->b()Lcom/kamcord/android/KC_p;

    move-result-object v3

    if-ne v3, p1, :cond_0

    invoke-virtual {v0}, La/a/a/c/KC_a;->e()I
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    move-result v0

    :goto_0
    return v0

    :catch_0
    move-exception v0

    move v0, v1

    goto :goto_0

    :cond_1
    move v0, v1

    goto :goto_0
.end method

.method public onBackPressed()V
    .locals 1

    const-string v0, "onBackPressed"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/KamcordActivity;->g:Lcom/kamcord/android/KC_T;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_T;->a()Z

    move-result v0

    if-nez v0, :cond_0

    invoke-super {p0}, La/a/a/a/KC_f;->onBackPressed()V

    :cond_0
    return-void
.end method

.method public onCreate(Landroid/os/Bundle;)V
    .locals 4

    const/4 v0, 0x0

    :try_start_0
    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    invoke-super {p0, p1}, La/a/a/a/KC_f;->onCreate(Landroid/os/Bundle;)V

    if-eqz p1, :cond_1

    invoke-static {p0}, Lcom/kamcord/android/Kamcord;->setApplicationContextFrom(Landroid/content/Context;)V

    const-string v0, "dev_key"

    invoke-virtual {p1, v0}, Landroid/os/Bundle;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    const-string v1, "dev_secret"

    invoke-virtual {p1, v1}, Landroid/os/Bundle;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    const-string v2, "app_name"

    invoke-virtual {p1, v2}, Landroid/os/Bundle;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    invoke-static {v0, v1, v2}, Lcom/kamcord/android/Kamcord;->initKeyAndSecret(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "mode"

    const/4 v1, 0x0

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->getInt(Ljava/lang/String;I)I

    move-result v0

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->i:I

    const-string v0, "orientation"

    const/4 v1, 0x0

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->getInt(Ljava/lang/String;I)I

    move-result v0

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->j:I

    const-string v0, "tab_index"

    const/4 v1, 0x0

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->getInt(Ljava/lang/String;I)I

    move-result v0

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->k:I

    :goto_0
    iget v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    const/4 v0, 0x1

    invoke-virtual {p0, v0}, Lcom/kamcord/android/KamcordActivity;->requestWindowFeature(I)Z

    iget v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    iget v0, p0, Lcom/kamcord/android/KamcordActivity;->j:I

    invoke-virtual {p0, v0}, Lcom/kamcord/android/KamcordActivity;->setRequestedOrientation(I)V

    iget v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    const-string v0, "layout"

    const-string v1, "z_kamcord_activity"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    iget v1, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    add-int/lit8 v1, v1, 0x1

    iput v1, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    invoke-virtual {p0, v0}, Lcom/kamcord/android/KamcordActivity;->setContentView(I)V

    iget v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    invoke-virtual {p0}, Lcom/kamcord/android/KamcordActivity;->getWindow()Landroid/view/Window;

    move-result-object v0

    const/16 v1, 0x400

    const/16 v2, 0x400

    invoke-virtual {v0, v1, v2}, Landroid/view/Window;->setFlags(II)V

    iget v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    const-string v0, "id"

    const-string v1, "tabbar_back"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {p0, v0}, Lcom/kamcord/android/KamcordActivity;->findViewById(I)Landroid/view/View;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/KamcordActivity$1;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KamcordActivity$1;-><init>(Lcom/kamcord/android/KamcordActivity;)V

    invoke-virtual {v0, v1}, Landroid/view/View;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    iget v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    new-instance v0, Lcom/kamcord/android/KC_q;

    iget v1, p0, Lcom/kamcord/android/KamcordActivity;->i:I

    iget v2, p0, Lcom/kamcord/android/KamcordActivity;->k:I

    invoke-direct {v0, p0, v1, v2}, Lcom/kamcord/android/KC_q;-><init>(Lcom/kamcord/android/KamcordActivity;II)V

    iput-object v0, p0, Lcom/kamcord/android/KamcordActivity;->h:Lcom/kamcord/android/KC_q;

    iget v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    const-string v0, "Initializing mTabFragmentManager..."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    new-instance v0, Lcom/kamcord/android/KC_T;

    invoke-virtual {p0}, Lcom/kamcord/android/KamcordActivity;->getSupportFragmentManager()La/a/a/a/KC_h;

    move-result-object v1

    iget-object v2, p0, Lcom/kamcord/android/KamcordActivity;->h:Lcom/kamcord/android/KC_q;

    invoke-virtual {v2}, Lcom/kamcord/android/KC_q;->size()I

    move-result v2

    invoke-direct {v0, v1, v2}, Lcom/kamcord/android/KC_T;-><init>(La/a/a/a/KC_h;I)V

    iput-object v0, p0, Lcom/kamcord/android/KamcordActivity;->g:Lcom/kamcord/android/KC_T;

    iget v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    new-instance v0, Lcom/kamcord/android/KamcordActivity$KC_a;

    invoke-virtual {p0}, Lcom/kamcord/android/KamcordActivity;->getSupportFragmentManager()La/a/a/a/KC_h;

    move-result-object v1

    iget-object v2, p0, Lcom/kamcord/android/KamcordActivity;->h:Lcom/kamcord/android/KC_q;

    iget v3, p0, Lcom/kamcord/android/KamcordActivity;->i:I

    invoke-direct {v0, v1, v2, v3}, Lcom/kamcord/android/KamcordActivity$KC_a;-><init>(La/a/a/a/KC_h;Lcom/kamcord/android/KC_q;I)V

    iput-object v0, p0, Lcom/kamcord/android/KamcordActivity;->e:Lcom/kamcord/android/KamcordActivity$KC_a;

    iget v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    const-string v0, "id"

    const-string v1, "pager"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {p0, v0}, Lcom/kamcord/android/KamcordActivity;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/CustomViewPager;

    iput-object v0, p0, Lcom/kamcord/android/KamcordActivity;->f:Lcom/kamcord/android/CustomViewPager;

    iget v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    iget-object v0, p0, Lcom/kamcord/android/KamcordActivity;->f:Lcom/kamcord/android/CustomViewPager;

    const/4 v1, 0x2

    invoke-virtual {v0, v1}, Lcom/kamcord/android/CustomViewPager;->setOffscreenPageLimit(I)V

    iget v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    iget-object v0, p0, Lcom/kamcord/android/KamcordActivity;->f:Lcom/kamcord/android/CustomViewPager;

    iget-object v1, p0, Lcom/kamcord/android/KamcordActivity;->e:Lcom/kamcord/android/KamcordActivity$KC_a;

    invoke-virtual {v0, v1}, Lcom/kamcord/android/CustomViewPager;->setAdapter(La/a/a/e/KC_e;)V

    iget v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    iget-object v0, p0, Lcom/kamcord/android/KamcordActivity;->h:Lcom/kamcord/android/KC_q;

    invoke-virtual {v0, p0}, Lcom/kamcord/android/KC_q;->a(Lcom/kamcord/android/KC_r;)V

    iget v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    iget-object v0, p0, Lcom/kamcord/android/KamcordActivity;->h:Lcom/kamcord/android/KC_q;

    iget-object v1, p0, Lcom/kamcord/android/KamcordActivity;->g:Lcom/kamcord/android/KC_T;

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_q;->a(Lcom/kamcord/android/KC_r;)V

    iget v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    const-string v0, "SANS_SERIF"

    const-string v1, "proximanova_light.otf"

    invoke-static {v0, v1}, Lcom/kamcord/android/KC_e;->a(Ljava/lang/String;Ljava/lang/String;)V

    if-eqz p1, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->notifyViewDidAppear()V

    :cond_0
    iget v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    const/4 v0, 0x1

    invoke-static {v0}, Lcom/kamcord/android/KC_m;->a(I)V

    :goto_1
    return-void

    :cond_1
    invoke-virtual {p0}, Lcom/kamcord/android/KamcordActivity;->getIntent()Landroid/content/Intent;

    move-result-object v0

    invoke-virtual {v0}, Landroid/content/Intent;->getExtras()Landroid/os/Bundle;

    move-result-object v0

    const-string v1, "mode"

    const/4 v2, 0x0

    invoke-virtual {v0, v1, v2}, Landroid/os/Bundle;->getInt(Ljava/lang/String;I)I

    move-result v0

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->i:I

    invoke-static {}, Lcom/kamcord/android/Kamcord;->a()I

    move-result v0

    iput v0, p0, Lcom/kamcord/android/KamcordActivity;->j:I
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    goto/16 :goto_0

    :catch_0
    move-exception v0

    const-string v1, "KamcordActivity"

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "There was an error in onCreate, progress = "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget v3, p0, Lcom/kamcord/android/KamcordActivity;->l:I

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/Exception;->printStackTrace()V

    invoke-virtual {p0}, Lcom/kamcord/android/KamcordActivity;->finish()V

    goto :goto_1
.end method

.method protected onDestroy()V
    .locals 0

    invoke-super {p0}, La/a/a/a/KC_f;->onDestroy()V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->notifyViewDidDisappear()V

    return-void
.end method

.method public onResume()V
    .locals 0

    invoke-super {p0}, La/a/a/a/KC_f;->onResume()V

    return-void
.end method

.method protected onSaveInstanceState(Landroid/os/Bundle;)V
    .locals 2

    const-string v0, "mode"

    iget v1, p0, Lcom/kamcord/android/KamcordActivity;->i:I

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putInt(Ljava/lang/String;I)V

    const-string v0, "orientation"

    iget v1, p0, Lcom/kamcord/android/KamcordActivity;->j:I

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putInt(Ljava/lang/String;I)V

    const-string v0, "tab_index"

    iget-object v1, p0, Lcom/kamcord/android/KamcordActivity;->h:Lcom/kamcord/android/KC_q;

    invoke-virtual {v1}, Lcom/kamcord/android/KC_q;->c()La/a/a/c/KC_a;

    move-result-object v1

    invoke-virtual {v1}, La/a/a/c/KC_a;->e()I

    move-result v1

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putInt(Ljava/lang/String;I)V

    const-string v0, "dev_key"

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getDeveloperKey()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putString(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "dev_secret"

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getDeveloperSecret()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putString(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "app_name"

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getApplicationName()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putString(Ljava/lang/String;Ljava/lang/String;)V

    invoke-super {p0, p1}, La/a/a/a/KC_f;->onSaveInstanceState(Landroid/os/Bundle;)V

    return-void
.end method

.method public onTabReselected$7f50f85c(La/a/a/c/KC_a;)V
    .locals 0

    return-void
.end method

.method public onTabSelected$7f50f85c(La/a/a/c/KC_a;)V
    .locals 3

    const-string v0, "onTabSelected"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/KamcordActivity;->f:Lcom/kamcord/android/CustomViewPager;

    invoke-virtual {p1}, La/a/a/c/KC_a;->e()I

    move-result v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/CustomViewPager;->setCurrentItem(I)V

    invoke-virtual {p1}, La/a/a/c/KC_a;->d()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, La/a/a/c/KC_a;->a()Z

    move-result v0

    if-nez v0, :cond_0

    new-instance v0, Lcom/kamcord/android/KC_x;

    invoke-direct {v0}, Lcom/kamcord/android/KC_x;-><init>()V

    invoke-virtual {p0}, Lcom/kamcord/android/KamcordActivity;->getFragmentManager()Landroid/app/FragmentManager;

    move-result-object v1

    const/4 v2, 0x0

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/android/KC_x;->show(Landroid/app/FragmentManager;Ljava/lang/String;)V

    :cond_0
    return-void
.end method

.method public onTabUnselected$7f50f85c(La/a/a/c/KC_a;)V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KamcordActivity;->g:Lcom/kamcord/android/KC_T;

    invoke-virtual {p1}, La/a/a/c/KC_a;->e()I

    move-result v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_T;->a(I)La/a/a/a/KC_e;

    move-result-object v0

    if-eqz v0, :cond_0

    instance-of v1, v0, Lcom/kamcord/android/KC_S;

    if-eqz v1, :cond_0

    check-cast v0, Lcom/kamcord/android/KC_S;

    iget-object v1, v0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    invoke-virtual {v1}, Lcom/kamcord/android/KC_Q;->canPause()Z

    move-result v1

    if-eqz v1, :cond_0

    iget-object v0, v0, Lcom/kamcord/android/KC_S;->M:Lcom/kamcord/android/KC_Q;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Q;->pause()V

    :cond_0
    return-void
.end method
