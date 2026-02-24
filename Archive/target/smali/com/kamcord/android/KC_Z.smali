.class public Lcom/kamcord/android/KC_Z;
.super La/a/a/a/KC_e;

# interfaces
.implements Landroid/view/TextureView$SurfaceTextureListener;


# instance fields
.field protected N:Lcom/kamcord/android/KC_w;

.field protected O:Lcom/kamcord/android/AspectRatioTextureView;

.field protected P:I

.field protected Q:Landroid/widget/FrameLayout;

.field protected R:Landroid/view/View;

.field protected S:Lcom/kamcord/android/CustomWebView;

.field protected T:Lorg/json/JSONObject;


# direct methods
.method public constructor <init>()V
    .locals 1

    invoke-direct {p0}, La/a/a/a/KC_e;-><init>()V

    const/4 v0, 0x0

    iput v0, p0, Lcom/kamcord/android/KC_Z;->P:I

    return-void
.end method

.method protected static a(Ljava/lang/String;)Ljava/lang/String;
    .locals 2

    const-string v0, "^https"

    const-string v1, "http"

    invoke-virtual {p0, v0, v1}, Ljava/lang/String;->replaceFirst(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    return-object v0
.end method


# virtual methods
.method protected final C()V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_Z;->N:Lcom/kamcord/android/KC_w;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_w;->b()Z

    move-result v0

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_Z;->N:Lcom/kamcord/android/KC_w;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_w;->d()V

    :goto_0
    return-void

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_Z;->N:Lcom/kamcord/android/KC_w;

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_w;->a(I)V

    goto :goto_0
.end method

.method public a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
    .locals 4
    .annotation build Landroid/annotation/SuppressLint;
        value = {
            "SetJavaScriptEnabled"
        }
    .end annotation

    const-string v0, "layout"

    const-string v1, "z_kamcord_fragment_player"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    const/4 v1, 0x0

    invoke-virtual {p1, v0, p2, v1}, Landroid/view/LayoutInflater;->inflate(ILandroid/view/ViewGroup;Z)Landroid/view/View;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_Z;->R:Landroid/view/View;

    invoke-virtual {p0}, Lcom/kamcord/android/KC_Z;->g()Landroid/os/Bundle;

    move-result-object v0

    :try_start_0
    new-instance v1, Lorg/json/JSONObject;

    const-string v2, "jsonString"

    const-string v3, ""

    invoke-virtual {v0, v2, v3}, Landroid/os/Bundle;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    invoke-direct {v1, v0}, Lorg/json/JSONObject;-><init>(Ljava/lang/String;)V

    iput-object v1, p0, Lcom/kamcord/android/KC_Z;->T:Lorg/json/JSONObject;
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    iget-object v0, p0, Lcom/kamcord/android/KC_Z;->R:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "videoSurfaceContainer"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/FrameLayout;

    iput-object v0, p0, Lcom/kamcord/android/KC_Z;->Q:Landroid/widget/FrameLayout;

    iget-object v0, p0, Lcom/kamcord/android/KC_Z;->R:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "intermediateBottom"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/CustomWebView;

    iput-object v0, p0, Lcom/kamcord/android/KC_Z;->S:Lcom/kamcord/android/CustomWebView;

    iget-object v0, p0, Lcom/kamcord/android/KC_Z;->R:Landroid/view/View;

    new-instance v1, Lcom/kamcord/android/KC_Z$1;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_Z$1;-><init>(Lcom/kamcord/android/KC_Z;)V

    invoke-virtual {v0, v1}, Landroid/view/View;->setOnTouchListener(Landroid/view/View$OnTouchListener;)V

    new-instance v0, Lcom/kamcord/android/KC_w;

    invoke-virtual {p0}, Lcom/kamcord/android/KC_Z;->h()La/a/a/a/KC_f;

    move-result-object v1

    invoke-direct {v0, v1}, Lcom/kamcord/android/KC_w;-><init>(Landroid/content/Context;)V

    iput-object v0, p0, Lcom/kamcord/android/KC_Z;->N:Lcom/kamcord/android/KC_w;

    iget-object v0, p0, Lcom/kamcord/android/KC_Z;->N:Lcom/kamcord/android/KC_w;

    iget-object v1, p0, Lcom/kamcord/android/KC_Z;->Q:Landroid/widget/FrameLayout;

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_w;->a(Landroid/widget/FrameLayout;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_Z;->R:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "surface_view"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/AspectRatioTextureView;

    iput-object v0, p0, Lcom/kamcord/android/KC_Z;->O:Lcom/kamcord/android/AspectRatioTextureView;

    iget-object v0, p0, Lcom/kamcord/android/KC_Z;->O:Lcom/kamcord/android/AspectRatioTextureView;

    invoke-virtual {v0, p0}, Lcom/kamcord/android/AspectRatioTextureView;->setSurfaceTextureListener(Landroid/view/TextureView$SurfaceTextureListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_Z;->R:Landroid/view/View;

    :goto_0
    return-object v0

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Lorg/json/JSONException;->printStackTrace()V

    new-instance v0, Lorg/json/JSONObject;

    invoke-direct {v0}, Lorg/json/JSONObject;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/KC_Z;->T:Lorg/json/JSONObject;

    new-instance v0, Lcom/kamcord/android/KC_j;

    const-string v1, "kamcordNoVideoAvailable"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    invoke-direct {v0, v1}, Lcom/kamcord/android/KC_j;-><init>(Ljava/lang/String;)V

    invoke-virtual {p0}, Lcom/kamcord/android/KC_Z;->h()La/a/a/a/KC_f;

    move-result-object v1

    invoke-virtual {v1}, La/a/a/a/KC_f;->getSupportFragmentManager()La/a/a/a/KC_h;

    move-result-object v1

    const-string v2, ""

    invoke-virtual {v0, v1, v2}, La/a/a/a/KC_d;->a(La/a/a/a/KC_h;Ljava/lang/String;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_Z;->R:Landroid/view/View;

    goto :goto_0
.end method

.method protected c(Z)V
    .locals 0

    return-void
.end method

.method public onSurfaceTextureAvailable(Landroid/graphics/SurfaceTexture;II)V
    .locals 0

    return-void
.end method

.method public onSurfaceTextureDestroyed(Landroid/graphics/SurfaceTexture;)Z
    .locals 1

    const/4 v0, 0x0

    return v0
.end method

.method public onSurfaceTextureSizeChanged(Landroid/graphics/SurfaceTexture;II)V
    .locals 0

    return-void
.end method

.method public onSurfaceTextureUpdated(Landroid/graphics/SurfaceTexture;)V
    .locals 0

    return-void
.end method
