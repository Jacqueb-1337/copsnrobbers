.class public Lcom/kamcord/android/WebActivity;
.super La/a/a/a/KC_f;


# instance fields
.field private e:Lcom/kamcord/android/KC_u;


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, La/a/a/a/KC_f;-><init>()V

    return-void
.end method


# virtual methods
.method public onBackPressed()V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/WebActivity;->e:Lcom/kamcord/android/KC_u;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_u;->b()V

    invoke-super {p0}, La/a/a/a/KC_f;->onBackPressed()V

    return-void
.end method

.method public onCreate(Landroid/os/Bundle;)V
    .locals 4

    const/high16 v3, 0x40000

    const/16 v2, 0x3021

    const/16 v1, 0x20

    invoke-super {p0, p1}, La/a/a/a/KC_f;->onCreate(Landroid/os/Bundle;)V

    invoke-virtual {p0}, Lcom/kamcord/android/WebActivity;->getWindow()Landroid/view/Window;

    move-result-object v0

    invoke-virtual {v0, v1, v1}, Landroid/view/Window;->setFlags(II)V

    invoke-virtual {p0}, Lcom/kamcord/android/WebActivity;->getWindow()Landroid/view/Window;

    move-result-object v0

    invoke-virtual {v0, v3, v3}, Landroid/view/Window;->setFlags(II)V

    const-string v0, "layout"

    const-string v1, "z_kamcord_activity_login"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {p0, v0}, Lcom/kamcord/android/WebActivity;->setContentView(I)V

    const-string v0, "id"

    const-string v1, "main"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {p0, v0}, Lcom/kamcord/android/WebActivity;->findViewById(I)Landroid/view/View;

    move-result-object v0

    invoke-virtual {v0, v2}, Landroid/view/View;->setId(I)V

    if-eqz p1, :cond_0

    :goto_0
    return-void

    :cond_0
    new-instance v0, Lcom/kamcord/android/KC_u;

    invoke-direct {v0}, Lcom/kamcord/android/KC_u;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/WebActivity;->e:Lcom/kamcord/android/KC_u;

    iget-object v0, p0, Lcom/kamcord/android/WebActivity;->e:Lcom/kamcord/android/KC_u;

    invoke-virtual {p0}, Lcom/kamcord/android/WebActivity;->getIntent()Landroid/content/Intent;

    move-result-object v1

    invoke-virtual {v1}, Landroid/content/Intent;->getExtras()Landroid/os/Bundle;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_u;->f(Landroid/os/Bundle;)V

    invoke-virtual {p0}, Lcom/kamcord/android/WebActivity;->getSupportFragmentManager()La/a/a/a/KC_h;

    move-result-object v0

    invoke-virtual {v0}, La/a/a/a/KC_h;->a()La/a/a/a/KC_m;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/android/WebActivity;->e:Lcom/kamcord/android/KC_u;

    invoke-virtual {v0, v2, v1}, La/a/a/a/KC_m;->a(ILa/a/a/a/KC_e;)La/a/a/a/KC_m;

    move-result-object v0

    invoke-virtual {v0}, La/a/a/a/KC_m;->a()I

    goto :goto_0
.end method

.method public onTouchEvent(Landroid/view/MotionEvent;)Z
    .locals 2

    const/4 v0, 0x4

    invoke-virtual {p1}, Landroid/view/MotionEvent;->getAction()I

    move-result v1

    if-ne v0, v1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/WebActivity;->e:Lcom/kamcord/android/KC_u;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_u;->b()V

    invoke-virtual {p0}, Lcom/kamcord/android/WebActivity;->finish()V

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    invoke-super {p0, p1}, La/a/a/a/KC_f;->onTouchEvent(Landroid/view/MotionEvent;)Z

    move-result v0

    goto :goto_0
.end method
