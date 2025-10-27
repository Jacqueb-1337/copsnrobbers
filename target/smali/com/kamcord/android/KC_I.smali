.class public Lcom/kamcord/android/KC_I;
.super La/a/a/a/KC_e;

# interfaces
.implements Lcom/kamcord/android/AuthListener;
.implements Lcom/kamcord/android/KC_H$KC_a;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/KC_I$KC_a;
    }
.end annotation


# instance fields
.field private M:Lcom/kamcord/android/KamcordActivity;

.field private N:Landroid/view/View;

.field private O:Ljava/util/List;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/List",
            "<",
            "Lcom/kamcord/android/b/KC_e;",
            ">;"
        }
    .end annotation
.end field

.field private P:Ljava/util/List;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/List",
            "<",
            "Landroid/widget/ImageView;",
            ">;"
        }
    .end annotation
.end field

.field private Q:Ljava/util/List;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/List",
            "<",
            "Landroid/widget/TextView;",
            ">;"
        }
    .end annotation
.end field

.field private R:Ljava/util/List;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/List",
            "<",
            "Landroid/widget/TextView;",
            ">;"
        }
    .end annotation
.end field

.field private S:Ljava/util/List;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/List",
            "<",
            "Landroid/widget/ToggleButton;",
            ">;"
        }
    .end annotation
.end field

.field private T:Landroid/widget/TextView;

.field private U:Landroid/widget/ToggleButton;


# direct methods
.method public constructor <init>()V
    .locals 1

    invoke-direct {p0}, La/a/a/a/KC_e;-><init>()V

    new-instance v0, Ljava/util/LinkedList;

    invoke-direct {v0}, Ljava/util/LinkedList;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/KC_I;->O:Ljava/util/List;

    new-instance v0, Ljava/util/LinkedList;

    invoke-direct {v0}, Ljava/util/LinkedList;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/KC_I;->P:Ljava/util/List;

    new-instance v0, Ljava/util/LinkedList;

    invoke-direct {v0}, Ljava/util/LinkedList;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/KC_I;->Q:Ljava/util/List;

    new-instance v0, Ljava/util/LinkedList;

    invoke-direct {v0}, Ljava/util/LinkedList;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/KC_I;->R:Ljava/util/List;

    new-instance v0, Ljava/util/LinkedList;

    invoke-direct {v0}, Ljava/util/LinkedList;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/KC_I;->S:Ljava/util/List;

    return-void
.end method

.method private B()V
    .locals 4

    const/4 v3, 0x0

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->N:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "voice_overlay_container"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/LinearLayout;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->voiceOverlayEnabled()Z

    move-result v1

    if-eqz v1, :cond_2

    invoke-virtual {v0, v3}, Landroid/widget/LinearLayout;->setVisibility(I)V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v0

    const-string v1, "voice_overlay_enabled"

    invoke-interface {v0, v1, v3}, Landroid/content/SharedPreferences;->getBoolean(Ljava/lang/String;Z)Z

    move-result v0

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->T:Landroid/widget/TextView;

    const-string v1, "string"

    const-string v2, "kamcordOn"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/TextView;->setText(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->U:Landroid/widget/ToggleButton;

    invoke-virtual {v0, v3}, Landroid/widget/ToggleButton;->setChecked(Z)V

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->U:Landroid/widget/ToggleButton;

    const-string v1, "string"

    const-string v2, "kamcordDisable"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/ToggleButton;->setText(I)V

    :goto_0
    return-void

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_I;->T:Landroid/widget/TextView;

    const-string v1, "string"

    const-string v2, "kamcordOff"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/TextView;->setText(I)V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->voiceOverlayEnabled()Z

    move-result v0

    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->U:Landroid/widget/ToggleButton;

    const/4 v1, 0x1

    invoke-virtual {v0, v1}, Landroid/widget/ToggleButton;->setChecked(Z)V

    :goto_1
    iget-object v0, p0, Lcom/kamcord/android/KC_I;->U:Landroid/widget/ToggleButton;

    const-string v1, "string"

    const-string v2, "kamcordEnable"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/ToggleButton;->setText(I)V

    goto :goto_0

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_I;->U:Landroid/widget/ToggleButton;

    invoke-virtual {v0, v3}, Landroid/widget/ToggleButton;->setChecked(Z)V

    goto :goto_1

    :cond_2
    const/16 v1, 0x8

    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->setVisibility(I)V

    goto :goto_0
.end method

.method static synthetic a(Lcom/kamcord/android/KC_I;)Lcom/kamcord/android/KamcordActivity;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->M:Lcom/kamcord/android/KamcordActivity;

    return-object v0
.end method

.method private b(Ljava/lang/String;)I
    .locals 2

    const/4 v0, 0x0

    move v1, v0

    :goto_0
    iget-object v0, p0, Lcom/kamcord/android/KC_I;->O:Ljava/util/List;

    invoke-interface {v0}, Ljava/util/List;->size()I

    move-result v0

    if-ge v1, v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->O:Ljava/util/List;

    invoke-interface {v0, v1}, Ljava/util/List;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/b/KC_e;

    invoke-virtual {v0}, Lcom/kamcord/android/b/KC_e;->d()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v0, p1}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_0

    :goto_1
    return v1

    :cond_0
    add-int/lit8 v0, v1, 0x1

    move v1, v0

    goto :goto_0

    :cond_1
    const/4 v1, -0x1

    goto :goto_1
.end method

.method static synthetic b(Lcom/kamcord/android/KC_I;)Landroid/view/View;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->N:Landroid/view/View;

    return-object v0
.end method

.method private b()V
    .locals 4

    const/4 v0, 0x0

    move v2, v0

    :goto_0
    iget-object v0, p0, Lcom/kamcord/android/KC_I;->O:Ljava/util/List;

    invoke-interface {v0}, Ljava/util/List;->size()I

    move-result v0

    if-ge v2, v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->P:Ljava/util/List;

    invoke-interface {v0, v2}, Ljava/util/List;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Landroid/widget/ImageView;

    iget-object v1, p0, Lcom/kamcord/android/KC_I;->O:Ljava/util/List;

    invoke-interface {v1, v2}, Ljava/util/List;->get(I)Ljava/lang/Object;

    move-result-object v1

    check-cast v1, Lcom/kamcord/android/b/KC_e;

    invoke-virtual {v1}, Lcom/kamcord/android/b/KC_e;->e()I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/ImageView;->setImageResource(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->Q:Ljava/util/List;

    invoke-interface {v0, v2}, Ljava/util/List;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Landroid/widget/TextView;

    iget-object v1, p0, Lcom/kamcord/android/KC_I;->O:Ljava/util/List;

    invoke-interface {v1, v2}, Ljava/util/List;->get(I)Ljava/lang/Object;

    move-result-object v1

    check-cast v1, Lcom/kamcord/android/b/KC_e;

    invoke-virtual {v1}, Lcom/kamcord/android/b/KC_e;->g()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Landroid/widget/TextView;->setText(Ljava/lang/CharSequence;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->O:Ljava/util/List;

    invoke-interface {v0, v2}, Ljava/util/List;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/b/KC_e;

    invoke-virtual {v0}, Lcom/kamcord/android/b/KC_e;->b()Z

    move-result v1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getAuthCenter()Lcom/kamcord/android/KC_e;

    move-result-object v3

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->O:Ljava/util/List;

    invoke-interface {v0, v2}, Ljava/util/List;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/b/KC_e;

    invoke-virtual {v0}, Lcom/kamcord/android/b/KC_e;->d()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v3, v0, v1}, Lcom/kamcord/android/KC_e;->a(Ljava/lang/String;Z)V

    if-eqz v1, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getAuthCenter()Lcom/kamcord/android/KC_e;

    move-result-object v1

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->O:Ljava/util/List;

    invoke-interface {v0, v2}, Ljava/util/List;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/b/KC_e;

    invoke-virtual {v0}, Lcom/kamcord/android/b/KC_e;->d()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v1, v0}, Lcom/kamcord/android/KC_e;->a(Ljava/lang/String;)V

    :cond_0
    add-int/lit8 v0, v2, 0x1

    move v2, v0

    goto :goto_0

    :cond_1
    return-void
.end method

.method static synthetic c(Lcom/kamcord/android/KC_I;)Ljava/util/List;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->R:Ljava/util/List;

    return-object v0
.end method

.method static synthetic d(Lcom/kamcord/android/KC_I;)Ljava/util/List;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->S:Ljava/util/List;

    return-object v0
.end method

.method static synthetic e(Lcom/kamcord/android/KC_I;)Ljava/util/List;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->O:Ljava/util/List;

    return-object v0
.end method


# virtual methods
.method public final a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
    .locals 9

    const/4 v0, 0x0

    const/4 v8, -0x1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getAuthCenter()Lcom/kamcord/android/KC_e;

    move-result-object v1

    invoke-virtual {v1, p0}, Lcom/kamcord/android/KC_e;->a(Lcom/kamcord/android/AuthListener;)V

    const-string v1, "layout"

    const-string v2, "z_kamcord_fragment_profile_settings"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {p1, v1, p2, v0}, Landroid/view/LayoutInflater;->inflate(ILandroid/view/ViewGroup;Z)Landroid/view/View;

    move-result-object v1

    iput-object v1, p0, Lcom/kamcord/android/KC_I;->N:Landroid/view/View;

    iget-object v1, p0, Lcom/kamcord/android/KC_I;->O:Ljava/util/List;

    new-instance v2, Lcom/kamcord/android/b/KC_c;

    iget-object v3, p0, Lcom/kamcord/android/KC_I;->M:Lcom/kamcord/android/KamcordActivity;

    invoke-direct {v2, v3}, Lcom/kamcord/android/b/KC_c;-><init>(Lcom/kamcord/android/KamcordActivity;)V

    invoke-interface {v1, v2}, Ljava/util/List;->add(Ljava/lang/Object;)Z

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getShareTargets()[I

    move-result-object v2

    :goto_0
    array-length v1, v2

    if-ge v0, v1, :cond_1

    aget v1, v2, v0

    packed-switch v1, :pswitch_data_0

    new-instance v1, Lcom/kamcord/android/b/KC_a;

    invoke-direct {v1}, Lcom/kamcord/android/b/KC_a;-><init>()V

    :goto_1
    invoke-virtual {v1}, Lcom/kamcord/android/b/KC_e;->f()Z

    move-result v3

    if-eqz v3, :cond_0

    iget-object v3, p0, Lcom/kamcord/android/KC_I;->O:Ljava/util/List;

    invoke-interface {v3, v1}, Ljava/util/List;->add(Ljava/lang/Object;)Z

    :cond_0
    add-int/lit8 v0, v0, 0x1

    goto :goto_0

    :pswitch_0
    new-instance v1, Lcom/kamcord/android/b/KC_b;

    invoke-direct {v1}, Lcom/kamcord/android/b/KC_b;-><init>()V

    goto :goto_1

    :pswitch_1
    new-instance v1, Lcom/kamcord/android/b/KC_f;

    invoke-direct {v1}, Lcom/kamcord/android/b/KC_f;-><init>()V

    goto :goto_1

    :pswitch_2
    new-instance v1, Lcom/kamcord/android/b/KC_g;

    invoke-direct {v1}, Lcom/kamcord/android/b/KC_g;-><init>()V

    goto :goto_1

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_I;->N:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "settings_items"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/LinearLayout;

    iget-object v1, p0, Lcom/kamcord/android/KC_I;->O:Ljava/util/List;

    invoke-interface {v1}, Ljava/util/List;->iterator()Ljava/util/Iterator;

    move-result-object v5

    :goto_2
    invoke-interface {v5}, Ljava/util/Iterator;->hasNext()Z

    move-result v1

    if-eqz v1, :cond_2

    invoke-interface {v5}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    const-string v1, "layout"

    const-string v2, "z_kamcord_merge_platform"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    const/4 v2, 0x1

    invoke-virtual {p1, v1, v0, v2}, Landroid/view/LayoutInflater;->inflate(ILandroid/view/ViewGroup;Z)Landroid/view/View;

    move-result-object v4

    const-string v1, "id"

    const-string v2, "icon"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v4, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v1

    check-cast v1, Landroid/widget/ImageView;

    const-string v2, "id"

    const-string v3, "network_name"

    invoke-static {v2, v3}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v4, v2}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v2

    check-cast v2, Landroid/widget/TextView;

    const-string v3, "id"

    const-string v6, "username"

    invoke-static {v3, v6}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v3

    invoke-virtual {v4, v3}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v3

    check-cast v3, Landroid/widget/TextView;

    const-string v6, "id"

    const-string v7, "sign_out"

    invoke-static {v6, v7}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v6

    invoke-virtual {v4, v6}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v4

    check-cast v4, Landroid/widget/ToggleButton;

    invoke-virtual {v1, v8}, Landroid/widget/ImageView;->setId(I)V

    invoke-virtual {v2, v8}, Landroid/widget/TextView;->setId(I)V

    invoke-virtual {v3, v8}, Landroid/widget/TextView;->setId(I)V

    invoke-virtual {v4, v8}, Landroid/widget/ToggleButton;->setId(I)V

    iget-object v6, p0, Lcom/kamcord/android/KC_I;->P:Ljava/util/List;

    invoke-interface {v6, v1}, Ljava/util/List;->add(Ljava/lang/Object;)Z

    iget-object v1, p0, Lcom/kamcord/android/KC_I;->Q:Ljava/util/List;

    invoke-interface {v1, v2}, Ljava/util/List;->add(Ljava/lang/Object;)Z

    iget-object v1, p0, Lcom/kamcord/android/KC_I;->R:Ljava/util/List;

    invoke-interface {v1, v3}, Ljava/util/List;->add(Ljava/lang/Object;)Z

    iget-object v1, p0, Lcom/kamcord/android/KC_I;->S:Ljava/util/List;

    invoke-interface {v1, v4}, Ljava/util/List;->add(Ljava/lang/Object;)Z

    goto :goto_2

    :cond_2
    invoke-virtual {v0}, Landroid/widget/LinearLayout;->getChildCount()I

    move-result v1

    add-int/lit8 v1, v1, -0x1

    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->getChildAt(I)Landroid/view/View;

    move-result-object v0

    const/16 v1, 0x8

    invoke-virtual {v0, v1}, Landroid/view/View;->setVisibility(I)V

    invoke-direct {p0}, Lcom/kamcord/android/KC_I;->b()V

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->N:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "voice_overlay_status_text"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/TextView;

    iput-object v0, p0, Lcom/kamcord/android/KC_I;->T:Landroid/widget/TextView;

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->N:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "voice_overlay_button"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/ToggleButton;

    iput-object v0, p0, Lcom/kamcord/android/KC_I;->U:Landroid/widget/ToggleButton;

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->U:Landroid/widget/ToggleButton;

    new-instance v1, Lcom/kamcord/android/KC_I$4;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_I$4;-><init>(Lcom/kamcord/android/KC_I;)V

    invoke-virtual {v0, v1}, Landroid/widget/ToggleButton;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    invoke-direct {p0}, Lcom/kamcord/android/KC_I;->B()V

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->N:Landroid/view/View;

    iput-object v0, p0, Lcom/kamcord/android/KC_I;->N:Landroid/view/View;

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->N:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "editProfileLayout"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/LinearLayout;

    new-instance v1, Lcom/kamcord/android/KC_I$1;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_I$1;-><init>(Lcom/kamcord/android/KC_I;)V

    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    new-instance v1, Lcom/kamcord/android/c/KC_a;

    invoke-direct {v1}, Lcom/kamcord/android/c/KC_a;-><init>()V

    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->setOnTouchListener(Landroid/view/View$OnTouchListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->N:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "termsOfServiceLayout"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/LinearLayout;

    new-instance v1, Lcom/kamcord/android/KC_I$2;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_I$2;-><init>(Lcom/kamcord/android/KC_I;)V

    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    new-instance v1, Lcom/kamcord/android/c/KC_a;

    invoke-direct {v1}, Lcom/kamcord/android/c/KC_a;-><init>()V

    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->setOnTouchListener(Landroid/view/View$OnTouchListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->N:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "privacyPolicyLayout"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/LinearLayout;

    new-instance v1, Lcom/kamcord/android/KC_I$3;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_I$3;-><init>(Lcom/kamcord/android/KC_I;)V

    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    new-instance v1, Lcom/kamcord/android/c/KC_a;

    invoke-direct {v1}, Lcom/kamcord/android/c/KC_a;-><init>()V

    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->setOnTouchListener(Landroid/view/View$OnTouchListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_I;->N:Landroid/view/View;

    return-object v0

    nop

    :pswitch_data_0
    .packed-switch 0x0
        :pswitch_0
        :pswitch_1
        :pswitch_2
    .end packed-switch
.end method

.method public final a(Landroid/app/Activity;)V
    .locals 0

    invoke-super {p0, p1}, La/a/a/a/KC_e;->a(Landroid/app/Activity;)V

    check-cast p1, Lcom/kamcord/android/KamcordActivity;

    iput-object p1, p0, Lcom/kamcord/android/KC_I;->M:Lcom/kamcord/android/KamcordActivity;

    return-void
.end method

.method public final a(Ljava/lang/String;)V
    .locals 3

    invoke-direct {p0, p1}, Lcom/kamcord/android/KC_I;->b(Ljava/lang/String;)I

    move-result v0

    if-gez v0, :cond_0

    :goto_0
    return-void

    :cond_0
    iget-object v1, p0, Lcom/kamcord/android/KC_I;->M:Lcom/kamcord/android/KamcordActivity;

    new-instance v2, Lcom/kamcord/android/KC_I$6;

    invoke-direct {v2, p0, v0}, Lcom/kamcord/android/KC_I$6;-><init>(Lcom/kamcord/android/KC_I;I)V

    invoke-virtual {v1, v2}, Lcom/kamcord/android/KamcordActivity;->runOnUiThread(Ljava/lang/Runnable;)V

    goto :goto_0
.end method

.method public final a(Ljava/lang/String;Z)V
    .locals 4

    invoke-direct {p0, p1}, Lcom/kamcord/android/KC_I;->b(Ljava/lang/String;)I

    move-result v0

    if-gez v0, :cond_1

    :cond_0
    :goto_0
    return-void

    :cond_1
    iget-object v1, p0, Lcom/kamcord/android/KC_I;->M:Lcom/kamcord/android/KamcordActivity;

    new-instance v2, Lcom/kamcord/android/KC_I$5;

    invoke-direct {v2, p0, p2, v0}, Lcom/kamcord/android/KC_I$5;-><init>(Lcom/kamcord/android/KC_I;ZI)V

    invoke-virtual {v1, v2}, Lcom/kamcord/android/KamcordActivity;->runOnUiThread(Ljava/lang/Runnable;)V

    const-string v0, "Kamcord"

    invoke-virtual {v0, p1}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_0

    if-eqz p2, :cond_2

    const/4 v0, 0x0

    :goto_1
    iget-object v1, p0, Lcom/kamcord/android/KC_I;->N:Landroid/view/View;

    const-string v2, "id"

    const-string v3, "editProfileLayout"

    invoke-static {v2, v3}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v1, v2}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v1

    invoke-virtual {v1, v0}, Landroid/view/View;->setVisibility(I)V

    iget-object v1, p0, Lcom/kamcord/android/KC_I;->N:Landroid/view/View;

    const-string v2, "id"

    const-string v3, "firstDivider"

    invoke-static {v2, v3}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v1, v2}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v1

    invoke-virtual {v1, v0}, Landroid/view/View;->setVisibility(I)V

    goto :goto_0

    :cond_2
    const/16 v0, 0x8

    goto :goto_1
.end method

.method public final b_()V
    .locals 0

    invoke-direct {p0}, Lcom/kamcord/android/KC_I;->B()V

    return-void
.end method

.method public final e()V
    .locals 1

    invoke-super {p0}, La/a/a/a/KC_e;->e()V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getAuthCenter()Lcom/kamcord/android/KC_e;

    move-result-object v0

    invoke-virtual {v0, p0}, Lcom/kamcord/android/KC_e;->b(Lcom/kamcord/android/AuthListener;)V

    return-void
.end method

.method public final o()V
    .locals 0

    invoke-super {p0}, La/a/a/a/KC_e;->o()V

    invoke-static {p0}, Lcom/kamcord/android/KC_H;->a(Lcom/kamcord/android/KC_H$KC_a;)V

    return-void
.end method

.method public final p()V
    .locals 0

    invoke-super {p0}, La/a/a/a/KC_e;->p()V

    invoke-static {p0}, Lcom/kamcord/android/KC_H;->b(Lcom/kamcord/android/KC_H$KC_a;)Z

    return-void
.end method

.method public reload()V
    .locals 0

    invoke-direct {p0}, Lcom/kamcord/android/KC_I;->b()V

    invoke-direct {p0}, Lcom/kamcord/android/KC_I;->B()V

    return-void
.end method
