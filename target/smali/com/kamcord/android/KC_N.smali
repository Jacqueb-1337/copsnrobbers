.class public final Lcom/kamcord/android/KC_N;
.super La/a/a/a/KC_e;

# interfaces
.implements Lcom/kamcord/android/AuthListener;
.implements Lcom/kamcord/android/KC_H$KC_a;
.implements Lcom/kamcord/android/VideoStatusListener;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/KC_N$KC_a;,
        Lcom/kamcord/android/KC_N$KC_b;
    }
.end annotation


# instance fields
.field M:Landroid/app/Activity;

.field N:Landroid/widget/ImageButton;

.field O:Landroid/widget/Button;

.field P:Landroid/widget/EditText;

.field Q:Landroid/widget/TextView;

.field R:Landroid/widget/ToggleButton;

.field S:[Landroid/widget/LinearLayout;

.field T:[Lcom/kamcord/android/b/KC_e;

.field U:Lcom/kamcord/android/core/KC_H;

.field private V:Landroid/view/View;

.field private W:Landroid/widget/ImageView;

.field private X:[Landroid/widget/TextView;

.field private Y:Ljava/util/HashMap;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/HashMap",
            "<",
            "Ljava/lang/String;",
            "Lcom/kamcord/android/b/KC_e;",
            ">;"
        }
    .end annotation
.end field

.field private Z:Landroid/widget/ProgressBar;

.field private aa:Lcom/kamcord/android/core/KC_H;

.field private ab:Z

.field private ac:Z

.field private ad:Z

.field private ae:Z

.field private af:I

.field private ag:Ljava/lang/String;

.field private ah:Ljava/lang/String;

.field private ai:Ljava/lang/String;


# direct methods
.method public constructor <init>()V
    .locals 4

    const/4 v3, 0x4

    const/4 v2, 0x0

    const/4 v1, 0x0

    invoke-direct {p0}, La/a/a/a/KC_e;-><init>()V

    new-array v0, v3, [Landroid/widget/LinearLayout;

    iput-object v0, p0, Lcom/kamcord/android/KC_N;->S:[Landroid/widget/LinearLayout;

    new-array v0, v3, [Landroid/widget/TextView;

    iput-object v0, p0, Lcom/kamcord/android/KC_N;->X:[Landroid/widget/TextView;

    new-array v0, v3, [Lcom/kamcord/android/b/KC_e;

    iput-object v0, p0, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    new-instance v0, Ljava/util/HashMap;

    invoke-direct {v0}, Ljava/util/HashMap;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/KC_N;->Y:Ljava/util/HashMap;

    iput-object v2, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    iput-boolean v1, p0, Lcom/kamcord/android/KC_N;->ab:Z

    iput-boolean v1, p0, Lcom/kamcord/android/KC_N;->ac:Z

    iput-boolean v1, p0, Lcom/kamcord/android/KC_N;->ad:Z

    iput-boolean v1, p0, Lcom/kamcord/android/KC_N;->ae:Z

    iput-object v2, p0, Lcom/kamcord/android/KC_N;->ag:Ljava/lang/String;

    iput-object v2, p0, Lcom/kamcord/android/KC_N;->ah:Ljava/lang/String;

    iput-object v2, p0, Lcom/kamcord/android/KC_N;->ai:Ljava/lang/String;

    return-void
.end method

.method private B()V
    .locals 4

    const/4 v0, 0x0

    :try_start_0
    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v1

    :goto_0
    const/4 v2, 0x4

    if-ge v0, v2, :cond_1

    iget-object v2, p0, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    aget-object v2, v2, v0

    if-eqz v2, :cond_0

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "shareOn"

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v3, p0, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    aget-object v3, v3, v0

    invoke-virtual {v3}, Lcom/kamcord/android/b/KC_e;->d()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    const/4 v3, 0x0

    invoke-interface {v1, v2, v3}, Landroid/content/SharedPreferences;->getBoolean(Ljava/lang/String;Z)Z

    move-result v2

    invoke-direct {p0, v0, v2}, Lcom/kamcord/android/KC_N;->a(IZ)V
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    :cond_0
    add-int/lit8 v0, v0, 0x1

    goto :goto_0

    :catch_0
    move-exception v0

    const-string v1, "Something went wrong in onResume()!"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V

    :cond_1
    return-void
.end method

.method private C()V
    .locals 4

    const/16 v3, 0x8

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->Q:Landroid/widget/TextView;

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Landroid/widget/TextView;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->Q:Landroid/widget/TextView;

    const-string v1, "string"

    const-string v2, "kamcordVoiceOverlay"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/TextView;->setText(I)V

    iget-boolean v0, p0, Lcom/kamcord/android/KC_N;->ab:Z

    if-eqz v0, :cond_2

    iget-boolean v0, p0, Lcom/kamcord/android/KC_N;->ad:Z

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->R:Landroid/widget/ToggleButton;

    iget-boolean v1, p0, Lcom/kamcord/android/KC_N;->ac:Z

    invoke-virtual {v0, v1}, Landroid/widget/ToggleButton;->setChecked(Z)V

    :goto_0
    return-void

    :cond_0
    iget-boolean v0, p0, Lcom/kamcord/android/KC_N;->ac:Z

    if-nez v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->Q:Landroid/widget/TextView;

    const-string v1, "string"

    const-string v2, "kamcordEnableVoiceOverlay"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/TextView;->setText(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->Q:Landroid/widget/TextView;

    new-instance v1, Lcom/kamcord/android/KC_N$9;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_N$9;-><init>(Lcom/kamcord/android/KC_N;)V

    invoke-virtual {v0, v1}, Landroid/widget/TextView;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->Q:Landroid/widget/TextView;

    new-instance v1, Lcom/kamcord/android/c/KC_a;

    invoke-direct {v1}, Lcom/kamcord/android/c/KC_a;-><init>()V

    invoke-virtual {v0, v1}, Landroid/widget/TextView;->setOnTouchListener(Landroid/view/View$OnTouchListener;)V

    :goto_1
    iget-object v0, p0, Lcom/kamcord/android/KC_N;->R:Landroid/widget/ToggleButton;

    invoke-virtual {v0, v3}, Landroid/widget/ToggleButton;->setVisibility(I)V

    goto :goto_0

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_N;->Q:Landroid/widget/TextView;

    const-string v1, "string"

    const-string v2, "kamcordVoiceOverlayEnabledExclamation"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/TextView;->setText(I)V

    goto :goto_1

    :cond_2
    iget-object v0, p0, Lcom/kamcord/android/KC_N;->V:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "voiceOverlayDivider"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    invoke-virtual {v0, v3}, Landroid/view/View;->setVisibility(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->V:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "voiceOverlayStatus"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    invoke-virtual {v0, v3}, Landroid/view/View;->setVisibility(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->R:Landroid/widget/ToggleButton;

    invoke-virtual {v0, v3}, Landroid/widget/ToggleButton;->setVisibility(I)V

    goto :goto_0
.end method

.method private a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;)Landroid/view/View;
    .locals 12

    const/4 v11, 0x4

    const/4 v10, 0x2

    const/4 v1, 0x0

    const-string v0, "layout"

    const-string v2, "z_kamcord_fragment_share"

    invoke-static {v0, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {p1, v0, p2, v1}, Landroid/view/LayoutInflater;->inflate(ILandroid/view/ViewGroup;Z)Landroid/view/View;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_N;->V:Landroid/view/View;

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->V:Landroid/view/View;

    const-string v2, "id"

    const-string v3, "thumbnailButton"

    invoke-static {v2, v3}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v0, v2}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/ImageButton;

    iput-object v0, p0, Lcom/kamcord/android/KC_N;->N:Landroid/widget/ImageButton;

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->V:Landroid/view/View;

    const-string v2, "id"

    const-string v3, "thumbnailPlayOverlay"

    invoke-static {v2, v3}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v0, v2}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/ImageView;

    iput-object v0, p0, Lcom/kamcord/android/KC_N;->W:Landroid/widget/ImageView;

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->V:Landroid/view/View;

    const-string v2, "id"

    const-string v3, "share"

    invoke-static {v2, v3}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v0, v2}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/Button;

    iput-object v0, p0, Lcom/kamcord/android/KC_N;->O:Landroid/widget/Button;

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->V:Landroid/view/View;

    const-string v2, "id"

    const-string v3, "description"

    invoke-static {v2, v3}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v0, v2}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/EditText;

    iput-object v0, p0, Lcom/kamcord/android/KC_N;->P:Landroid/widget/EditText;

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->P:Landroid/widget/EditText;

    new-instance v2, Lcom/kamcord/android/KC_N$6;

    invoke-direct {v2, p0}, Lcom/kamcord/android/KC_N$6;-><init>(Lcom/kamcord/android/KC_N;)V

    invoke-virtual {v0, v2}, Landroid/widget/EditText;->setOnFocusChangeListener(Landroid/view/View$OnFocusChangeListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->V:Landroid/view/View;

    const-string v2, "id"

    const-string v3, "voiceOverlayStatus"

    invoke-static {v2, v3}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v0, v2}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/TextView;

    iput-object v0, p0, Lcom/kamcord/android/KC_N;->Q:Landroid/widget/TextView;

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->V:Landroid/view/View;

    const-string v2, "id"

    const-string v3, "voiceOverlayToggle"

    invoke-static {v2, v3}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v0, v2}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/ToggleButton;

    iput-object v0, p0, Lcom/kamcord/android/KC_N;->R:Landroid/widget/ToggleButton;

    invoke-direct {p0}, Lcom/kamcord/android/KC_N;->C()V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->R:Landroid/widget/ToggleButton;

    new-instance v2, Lcom/kamcord/android/KC_N$7;

    invoke-direct {v2, p0}, Lcom/kamcord/android/KC_N$7;-><init>(Lcom/kamcord/android/KC_N;)V

    invoke-virtual {v0, v2}, Landroid/widget/ToggleButton;->setOnCheckedChangeListener(Landroid/widget/CompoundButton$OnCheckedChangeListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->V:Landroid/view/View;

    const-string v2, "id"

    const-string v3, "touchInterceptor"

    invoke-static {v2, v3}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v0, v2}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/FrameLayout;

    new-instance v2, Lcom/kamcord/android/KC_N$8;

    invoke-direct {v2, p0}, Lcom/kamcord/android/KC_N$8;-><init>(Lcom/kamcord/android/KC_N;)V

    invoke-virtual {v0, v2}, Landroid/widget/FrameLayout;->setOnTouchListener(Landroid/view/View$OnTouchListener;)V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getShareTargets()[I

    move-result-object v4

    move v2, v1

    :goto_0
    if-ge v2, v11, :cond_1

    array-length v0, v4

    if-ge v2, v0, :cond_0

    aget v0, v4, v2

    :goto_1
    packed-switch v0, :pswitch_data_0

    :goto_2
    :pswitch_0
    add-int/lit8 v2, v2, 0x1

    goto :goto_0

    :cond_0
    move v0, v2

    goto :goto_1

    :pswitch_1
    iget-object v0, p0, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    new-instance v3, Lcom/kamcord/android/b/KC_b;

    invoke-direct {v3}, Lcom/kamcord/android/b/KC_b;-><init>()V

    aput-object v3, v0, v2

    goto :goto_2

    :pswitch_2
    iget-object v0, p0, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    new-instance v3, Lcom/kamcord/android/b/KC_f;

    invoke-direct {v3}, Lcom/kamcord/android/b/KC_f;-><init>()V

    aput-object v3, v0, v2

    goto :goto_2

    :pswitch_3
    iget-object v0, p0, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    new-instance v3, Lcom/kamcord/android/b/KC_g;

    invoke-direct {v3}, Lcom/kamcord/android/b/KC_g;-><init>()V

    aput-object v3, v0, v2

    goto :goto_2

    :pswitch_4
    iget-object v0, p0, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    new-instance v3, Lcom/kamcord/android/b/KC_d;

    invoke-direct {v3}, Lcom/kamcord/android/b/KC_d;-><init>()V

    aput-object v3, v0, v2

    goto :goto_2

    :pswitch_5
    iget-object v0, p0, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    new-instance v3, Lcom/kamcord/android/b/KC_a;

    invoke-direct {v3}, Lcom/kamcord/android/b/KC_a;-><init>()V

    aput-object v3, v0, v2

    goto :goto_2

    :cond_1
    move v3, v1

    :goto_3
    if-ge v3, v10, :cond_4

    move v2, v1

    :goto_4
    if-ge v2, v10, :cond_3

    iget-object v5, p0, Lcom/kamcord/android/KC_N;->S:[Landroid/widget/LinearLayout;

    mul-int/lit8 v0, v3, 0x2

    add-int v6, v0, v2

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->V:Landroid/view/View;

    const-string v7, "id"

    new-instance v8, Ljava/lang/StringBuilder;

    const-string v9, "share_"

    invoke-direct {v8, v9}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v8, v3}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v8

    const-string v9, "_"

    invoke-virtual {v8, v9}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v8

    invoke-virtual {v8, v2}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v8

    invoke-virtual {v8}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v8

    invoke-static {v7, v8}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v7

    invoke-virtual {v0, v7}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/LinearLayout;

    aput-object v0, v5, v6

    iget-object v5, p0, Lcom/kamcord/android/KC_N;->X:[Landroid/widget/TextView;

    mul-int/lit8 v0, v3, 0x2

    add-int v6, v0, v2

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->V:Landroid/view/View;

    const-string v7, "id"

    new-instance v8, Ljava/lang/StringBuilder;

    const-string v9, "share_"

    invoke-direct {v8, v9}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v8, v3}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v8

    const-string v9, "_"

    invoke-virtual {v8, v9}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v8

    invoke-virtual {v8, v2}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v8

    const-string v9, "_text"

    invoke-virtual {v8, v9}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v8

    invoke-virtual {v8}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v8

    invoke-static {v7, v8}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v7

    invoke-virtual {v0, v7}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/TextView;

    aput-object v0, v5, v6

    mul-int/lit8 v0, v3, 0x2

    add-int/2addr v0, v2

    aget v0, v4, v0

    const/4 v5, -0x1

    if-eq v0, v5, :cond_2

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->X:[Landroid/widget/TextView;

    mul-int/lit8 v5, v3, 0x2

    add-int/2addr v5, v2

    aget-object v0, v0, v5

    iget-object v5, p0, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    mul-int/lit8 v6, v3, 0x2

    add-int/2addr v6, v2

    aget-object v5, v5, v6

    invoke-virtual {v5}, Lcom/kamcord/android/b/KC_e;->g()Ljava/lang/String;

    move-result-object v5

    sget-object v6, Ljava/util/Locale;->ENGLISH:Ljava/util/Locale;

    invoke-virtual {v5, v6}, Ljava/lang/String;->toUpperCase(Ljava/util/Locale;)Ljava/lang/String;

    move-result-object v5

    invoke-virtual {v0, v5}, Landroid/widget/TextView;->setText(Ljava/lang/CharSequence;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->Y:Ljava/util/HashMap;

    iget-object v5, p0, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    mul-int/lit8 v6, v3, 0x2

    add-int/2addr v6, v2

    aget-object v5, v5, v6

    invoke-virtual {v5}, Lcom/kamcord/android/b/KC_e;->d()Ljava/lang/String;

    move-result-object v5

    invoke-virtual {v5}, Ljava/lang/String;->toLowerCase()Ljava/lang/String;

    move-result-object v5

    iget-object v6, p0, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    mul-int/lit8 v7, v3, 0x2

    add-int/2addr v7, v2

    aget-object v6, v6, v7

    invoke-virtual {v0, v5, v6}, Ljava/util/HashMap;->put(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;

    :goto_5
    add-int/lit8 v0, v2, 0x1

    move v2, v0

    goto/16 :goto_4

    :cond_2
    iget-object v0, p0, Lcom/kamcord/android/KC_N;->S:[Landroid/widget/LinearLayout;

    mul-int/lit8 v5, v3, 0x2

    add-int/2addr v5, v2

    aget-object v0, v0, v5

    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->setEnabled(Z)V

    goto :goto_5

    :cond_3
    add-int/lit8 v0, v3, 0x1

    move v3, v0

    goto/16 :goto_3

    :cond_4
    iget-object v0, p0, Lcom/kamcord/android/KC_N;->V:Landroid/view/View;

    const-string v2, "id"

    const-string v3, "videoProgressBar"

    invoke-static {v2, v3}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v0, v2}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/ProgressBar;

    iput-object v0, p0, Lcom/kamcord/android/KC_N;->Z:Landroid/widget/ProgressBar;

    invoke-direct {p0}, Lcom/kamcord/android/KC_N;->B()V

    move v0, v1

    :goto_6
    if-ge v0, v11, :cond_5

    iget-object v1, p0, Lcom/kamcord/android/KC_N;->S:[Landroid/widget/LinearLayout;

    aget-object v1, v1, v0

    new-instance v2, Lcom/kamcord/android/KC_N$10;

    invoke-direct {v2, p0, v0}, Lcom/kamcord/android/KC_N$10;-><init>(Lcom/kamcord/android/KC_N;I)V

    invoke-virtual {v1, v2}, Landroid/widget/LinearLayout;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    add-int/lit8 v0, v0, 0x1

    goto :goto_6

    :cond_5
    iget-object v0, p0, Lcom/kamcord/android/KC_N;->V:Landroid/view/View;

    return-object v0

    nop

    :pswitch_data_0
    .packed-switch 0x0
        :pswitch_1
        :pswitch_2
        :pswitch_3
        :pswitch_5
        :pswitch_0
        :pswitch_4
    .end packed-switch
.end method

.method private a(IZ)V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->M:Landroid/app/Activity;

    new-instance v1, Lcom/kamcord/android/KC_N$1;

    invoke-direct {v1, p0, p1, p2}, Lcom/kamcord/android/KC_N$1;-><init>(Lcom/kamcord/android/KC_N;IZ)V

    invoke-virtual {v0, v1}, Landroid/app/Activity;->runOnUiThread(Ljava/lang/Runnable;)V

    return-void
.end method

.method static synthetic a(Lcom/kamcord/android/KC_N;IZ)V
    .locals 1

    const/4 v0, 0x0

    invoke-direct {p0, p1, v0}, Lcom/kamcord/android/KC_N;->a(IZ)V

    return-void
.end method

.method static synthetic a(Lcom/kamcord/android/KC_N;I)Z
    .locals 5

    const/4 v1, 0x1

    const/4 v2, 0x0

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    aget-object v0, v0, p1

    if-eqz v0, :cond_1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v0

    new-instance v3, Ljava/lang/StringBuilder;

    const-string v4, "shareOn"

    invoke-direct {v3, v4}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v4, p0, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    aget-object v4, v4, p1

    invoke-virtual {v4}, Lcom/kamcord/android/b/KC_e;->d()Ljava/lang/String;

    move-result-object v4

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    invoke-virtual {v3}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v3

    invoke-interface {v0, v3, v2}, Landroid/content/SharedPreferences;->getBoolean(Ljava/lang/String;Z)Z

    move-result v3

    if-nez v3, :cond_0

    move v0, v1

    :goto_0
    invoke-direct {p0, p1, v0}, Lcom/kamcord/android/KC_N;->a(IZ)V

    if-nez v3, :cond_1

    :goto_1
    return v1

    :cond_0
    move v0, v2

    goto :goto_0

    :cond_1
    move v1, v2

    goto :goto_1
.end method


# virtual methods
.method public final a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
    .locals 7

    const/4 v1, 0x0

    const/4 v2, 0x0

    const/4 v0, 0x0

    :try_start_0
    iput v0, p0, Lcom/kamcord/android/KC_N;->af:I

    if-eqz p3, :cond_1

    new-instance v0, Lcom/kamcord/android/core/KC_H;

    const-string v3, "video_share_local_id"

    const-string v4, ""

    invoke-virtual {p3, v3, v4}, Landroid/os/Bundle;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    const-string v4, "video_directory_path"

    const-string v5, ""

    invoke-virtual {p3, v4, v5}, Landroid/os/Bundle;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v4

    invoke-direct {v0, v3, v4}, Lcom/kamcord/android/core/KC_H;-><init>(Ljava/lang/String;Ljava/lang/String;)V

    iput-object v0, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    const-string v3, "video_share_ready"

    const/4 v4, 0x0

    invoke-virtual {p3, v3, v4}, Landroid/os/Bundle;->getBoolean(Ljava/lang/String;Z)Z

    move-result v3

    iput-boolean v3, v0, Lcom/kamcord/android/core/KC_H;->c:Z

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    const-string v3, "video_share_muxer_will_finish"

    const/4 v4, 0x0

    invoke-virtual {p3, v3, v4}, Landroid/os/Bundle;->getBoolean(Ljava/lang/String;Z)Z

    move-result v3

    iput-boolean v3, v0, Lcom/kamcord/android/core/KC_H;->d:Z

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    const-string v3, "video_share_successfully_uploaded"

    const/4 v4, 0x0

    invoke-virtual {p3, v3, v4}, Landroid/os/Bundle;->getBoolean(Ljava/lang/String;Z)Z

    move-result v3

    iput-boolean v3, v0, Lcom/kamcord/android/core/KC_H;->e:Z

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    const-string v3, "video_share_title"

    const/4 v4, 0x0

    invoke-virtual {p3, v3, v4}, Landroid/os/Bundle;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    iput-object v3, v0, Lcom/kamcord/android/core/KC_H;->f:Ljava/lang/String;
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_1

    :try_start_1
    iget-object v0, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    new-instance v3, Lorg/json/JSONObject;

    const-string v4, "video_share_metadata"

    const-string v5, "{}"

    invoke-virtual {p3, v4, v5}, Landroid/os/Bundle;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v4

    invoke-direct {v3, v4}, Lorg/json/JSONObject;-><init>(Ljava/lang/String;)V

    iput-object v3, v0, Lcom/kamcord/android/core/KC_H;->g:Lorg/json/JSONObject;
    :try_end_1
    .catch Lorg/json/JSONException; {:try_start_1 .. :try_end_1} :catch_0
    .catch Ljava/lang/Exception; {:try_start_1 .. :try_end_1} :catch_1

    :goto_0
    :try_start_2
    iget-object v0, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    const-string v3, "video_share_duration"

    const-wide/16 v4, 0x0

    invoke-virtual {p3, v3, v4, v5}, Landroid/os/Bundle;->getDouble(Ljava/lang/String;D)D

    move-result-wide v4

    iput-wide v4, v0, Lcom/kamcord/android/core/KC_H;->h:D

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    const-string v3, "video_share_server_id"

    const/4 v4, 0x0

    invoke-virtual {p3, v3, v4}, Landroid/os/Bundle;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    iput-object v3, v0, Lcom/kamcord/android/core/KC_H;->i:Ljava/lang/String;

    new-instance v0, Lcom/kamcord/android/core/KC_H;

    const-string v3, "video_local_id"

    const-string v4, ""

    invoke-virtual {p3, v3, v4}, Landroid/os/Bundle;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    const-string v4, "video_directory_path"

    const-string v5, ""

    invoke-virtual {p3, v4, v5}, Landroid/os/Bundle;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v4

    invoke-direct {v0, v3, v4}, Lcom/kamcord/android/core/KC_H;-><init>(Ljava/lang/String;Ljava/lang/String;)V

    iput-object v0, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    const-string v3, "video_ready"

    const/4 v4, 0x0

    invoke-virtual {p3, v3, v4}, Landroid/os/Bundle;->getBoolean(Ljava/lang/String;Z)Z

    move-result v3

    iput-boolean v3, v0, Lcom/kamcord/android/core/KC_H;->c:Z

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    const-string v3, "video_muxer_will_finish"

    const/4 v4, 0x0

    invoke-virtual {p3, v3, v4}, Landroid/os/Bundle;->getBoolean(Ljava/lang/String;Z)Z

    move-result v3

    iput-boolean v3, v0, Lcom/kamcord/android/core/KC_H;->d:Z

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    const-string v3, "video_successfully_uploaded"

    const/4 v4, 0x0

    invoke-virtual {p3, v3, v4}, Landroid/os/Bundle;->getBoolean(Ljava/lang/String;Z)Z

    move-result v3

    iput-boolean v3, v0, Lcom/kamcord/android/core/KC_H;->e:Z

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    const-string v3, "video_title"

    const/4 v4, 0x0

    invoke-virtual {p3, v3, v4}, Landroid/os/Bundle;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    iput-object v3, v0, Lcom/kamcord/android/core/KC_H;->f:Ljava/lang/String;
    :try_end_2
    .catch Ljava/lang/Exception; {:try_start_2 .. :try_end_2} :catch_1

    :try_start_3
    iget-object v0, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    new-instance v3, Lorg/json/JSONObject;

    const-string v4, "video_metadata"

    const-string v5, "{}"

    invoke-virtual {p3, v4, v5}, Landroid/os/Bundle;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v4

    invoke-direct {v3, v4}, Lorg/json/JSONObject;-><init>(Ljava/lang/String;)V

    iput-object v3, v0, Lcom/kamcord/android/core/KC_H;->g:Lorg/json/JSONObject;
    :try_end_3
    .catch Lorg/json/JSONException; {:try_start_3 .. :try_end_3} :catch_2
    .catch Ljava/lang/Exception; {:try_start_3 .. :try_end_3} :catch_1

    :goto_1
    :try_start_4
    iget-object v0, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    const-string v3, "video_duration"

    const-wide/16 v4, 0x0

    invoke-virtual {p3, v3, v4, v5}, Landroid/os/Bundle;->getDouble(Ljava/lang/String;D)D

    move-result-wide v4

    iput-wide v4, v0, Lcom/kamcord/android/core/KC_H;->h:D

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    const-string v3, "video_server_id"

    const/4 v4, 0x0

    invoke-virtual {p3, v3, v4}, Landroid/os/Bundle;->getString(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v3

    iput-object v3, v0, Lcom/kamcord/android/core/KC_H;->i:Ljava/lang/String;

    const-string v0, "vo_dev_enabled"

    invoke-virtual {p3, v0}, Landroid/os/Bundle;->getBoolean(Ljava/lang/String;)Z

    move-result v0

    iput-boolean v0, p0, Lcom/kamcord/android/KC_N;->ab:Z

    const-string v0, "vo_player_enabled"

    invoke-virtual {p3, v0}, Landroid/os/Bundle;->getBoolean(Ljava/lang/String;)Z

    move-result v0

    iput-boolean v0, p0, Lcom/kamcord/android/KC_N;->ac:Z

    const-string v0, "vo_track_exists"

    invoke-virtual {p3, v0}, Landroid/os/Bundle;->getBoolean(Ljava/lang/String;)Z

    move-result v0

    iput-boolean v0, p0, Lcom/kamcord/android/KC_N;->ad:Z

    const-string v0, "line_and_email"

    invoke-virtual {p3, v0}, Landroid/os/Bundle;->getBoolean(Ljava/lang/String;)Z

    move-result v0

    iput-boolean v0, p0, Lcom/kamcord/android/KC_N;->ae:Z

    iget-boolean v0, p0, Lcom/kamcord/android/KC_N;->ad:Z
    :try_end_4
    .catch Ljava/lang/Exception; {:try_start_4 .. :try_end_4} :catch_1

    if-eqz v0, :cond_0

    :try_start_5
    const-string v0, "kamcordcore"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V
    :try_end_5
    .catch Ljava/lang/Throwable; {:try_start_5 .. :try_end_5} :catch_3
    .catch Ljava/lang/Exception; {:try_start_5 .. :try_end_5} :catch_1

    :cond_0
    :goto_2
    :try_start_6
    iget v0, p0, Lcom/kamcord/android/KC_N;->af:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/KC_N;->af:I

    invoke-static {p0}, Lcom/kamcord/android/KC_H;->a(Lcom/kamcord/android/KC_H$KC_a;)V

    invoke-static {p0}, Lcom/kamcord/android/core/KC_S;->a(Lcom/kamcord/android/VideoStatusListener;)V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getAuthCenter()Lcom/kamcord/android/KC_e;

    move-result-object v0

    invoke-virtual {v0, p0}, Lcom/kamcord/android/KC_e;->a(Lcom/kamcord/android/AuthListener;)V

    iget v0, p0, Lcom/kamcord/android/KC_N;->af:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, Lcom/kamcord/android/KC_N;->af:I

    invoke-direct {p0, p1, p2}, Lcom/kamcord/android/KC_N;->a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;)Landroid/view/View;

    move-result-object v0

    iget v2, p0, Lcom/kamcord/android/KC_N;->af:I

    add-int/lit8 v2, v2, 0x1

    iput v2, p0, Lcom/kamcord/android/KC_N;->af:I

    iget-object v2, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    iget-boolean v2, v2, Lcom/kamcord/android/core/KC_H;->c:Z

    if-nez v2, :cond_2

    iget-object v2, p0, Lcom/kamcord/android/KC_N;->N:Landroid/widget/ImageButton;

    const/4 v3, 0x0

    invoke-virtual {v2, v3}, Landroid/widget/ImageButton;->setVisibility(I)V

    iget-object v2, p0, Lcom/kamcord/android/KC_N;->W:Landroid/widget/ImageView;

    const/16 v3, 0x8

    invoke-virtual {v2, v3}, Landroid/widget/ImageView;->setVisibility(I)V

    iget-object v2, p0, Lcom/kamcord/android/KC_N;->Z:Landroid/widget/ProgressBar;

    const/4 v3, 0x0

    invoke-virtual {v2, v3}, Landroid/widget/ProgressBar;->setVisibility(I)V

    iget-object v2, p0, Lcom/kamcord/android/KC_N;->O:Landroid/widget/Button;

    const-string v3, "string"

    const-string v4, "kamcordWorking"

    invoke-static {v3, v4}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v3

    invoke-virtual {v2, v3}, Landroid/widget/Button;->setText(I)V

    iget-object v2, p0, Lcom/kamcord/android/KC_N;->O:Landroid/widget/Button;

    const/4 v3, 0x0

    invoke-virtual {v2, v3}, Landroid/widget/Button;->setEnabled(Z)V

    :goto_3
    return-object v0

    :catch_0
    move-exception v0

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    new-instance v3, Lorg/json/JSONObject;

    invoke-direct {v3}, Lorg/json/JSONObject;-><init>()V

    iput-object v3, v0, Lcom/kamcord/android/core/KC_H;->g:Lorg/json/JSONObject;
    :try_end_6
    .catch Ljava/lang/Exception; {:try_start_6 .. :try_end_6} :catch_1

    goto/16 :goto_0

    :catch_1
    move-exception v0

    const-string v2, "ShareFragment"

    new-instance v3, Ljava/lang/StringBuilder;

    const-string v4, "There was an error in onCreateView, progress = "

    invoke-direct {v3, v4}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget v4, p0, Lcom/kamcord/android/KC_N;->af:I

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v3

    invoke-virtual {v3}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v3

    invoke-static {v2, v3}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/Exception;->printStackTrace()V

    move-object v0, v1

    goto :goto_3

    :catch_2
    move-exception v0

    :try_start_7
    iget-object v0, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    new-instance v3, Lorg/json/JSONObject;

    invoke-direct {v3}, Lorg/json/JSONObject;-><init>()V

    iput-object v3, v0, Lcom/kamcord/android/core/KC_H;->g:Lorg/json/JSONObject;

    goto/16 :goto_1

    :catch_3
    move-exception v0

    const-string v3, "Couldn\'t load libkamcordcore! Disabling voice track."

    invoke-static {v3}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V

    move-object v0, p0

    :goto_4
    move v6, v2

    move-object v2, v0

    move v0, v6

    :goto_5
    iput-boolean v0, v2, Lcom/kamcord/android/KC_N;->ad:Z

    goto/16 :goto_2

    :cond_1
    invoke-static {}, Lcom/kamcord/android/Kamcord;->b()Lcom/kamcord/android/core/KC_H;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->voiceOverlayEnabled()Z

    move-result v0

    iput-boolean v0, p0, Lcom/kamcord/android/KC_N;->ab:Z

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v0

    const-string v3, "voice_overlay_enabled"

    const/4 v4, 0x0

    invoke-interface {v0, v3, v4}, Landroid/content/SharedPreferences;->getBoolean(Ljava/lang/String;Z)Z

    move-result v0

    iput-boolean v0, p0, Lcom/kamcord/android/KC_N;->ac:Z

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    if-eqz v0, :cond_3

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_H;->b()Z

    move-result v0

    if-eqz v0, :cond_3

    const/4 v0, 0x1

    move-object v2, p0

    goto :goto_5

    :cond_2
    iget-object v2, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    invoke-virtual {p0, v2}, Lcom/kamcord/android/KC_N;->a(Lcom/kamcord/android/core/KC_H;)V
    :try_end_7
    .catch Ljava/lang/Exception; {:try_start_7 .. :try_end_7} :catch_1

    goto :goto_3

    :cond_3
    move-object v0, p0

    goto :goto_4
.end method

.method public final a(IILandroid/content/Intent;)V
    .locals 4

    const/4 v3, 0x0

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "onActivityResult("

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, ", "

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, p2}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, ", "

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, p3}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, ")"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    const/16 v0, 0x3039

    if-ne p1, v0, :cond_0

    iget-boolean v0, p0, Lcom/kamcord/android/KC_N;->ae:Z

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_H;->a:Ljava/lang/String;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_H;->a:Ljava/lang/String;

    iget-object v1, p0, Lcom/kamcord/android/KC_N;->ag:Ljava/lang/String;

    invoke-virtual {v0, v1}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->Y:Ljava/util/HashMap;

    const-string v1, "line"

    invoke-virtual {v0, v1}, Ljava/util/HashMap;->get(Ljava/lang/Object;)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/b/KC_e;

    iget-object v1, p0, Lcom/kamcord/android/KC_N;->ah:Ljava/lang/String;

    iget-object v2, p0, Lcom/kamcord/android/KC_N;->ai:Ljava/lang/String;

    invoke-virtual {v0, p0, v1, v2}, Lcom/kamcord/android/b/KC_e;->a(La/a/a/a/KC_e;Ljava/lang/String;Ljava/lang/String;)V

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/KC_N;->ae:Z

    iput-object v3, p0, Lcom/kamcord/android/KC_N;->ag:Ljava/lang/String;

    iput-object v3, p0, Lcom/kamcord/android/KC_N;->ah:Ljava/lang/String;

    iput-object v3, p0, Lcom/kamcord/android/KC_N;->ai:Ljava/lang/String;

    :cond_0
    return-void
.end method

.method public final a(Landroid/app/Activity;)V
    .locals 0

    invoke-super {p0, p1}, La/a/a/a/KC_e;->a(Landroid/app/Activity;)V

    iput-object p1, p0, Lcom/kamcord/android/KC_N;->M:Landroid/app/Activity;

    return-void
.end method

.method public final a(Landroid/os/Bundle;)V
    .locals 0

    invoke-super {p0, p1}, La/a/a/a/KC_e;->a(Landroid/os/Bundle;)V

    return-void
.end method

.method final declared-synchronized a(Lcom/kamcord/android/core/KC_H;)V
    .locals 3

    monitor-enter p0

    :try_start_0
    iput-object p1, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    new-instance v0, Ljava/lang/Thread;

    new-instance v1, Lcom/kamcord/android/KC_N$11;

    invoke-direct {v1, p0, p1}, Lcom/kamcord/android/KC_N$11;-><init>(Lcom/kamcord/android/KC_N;Lcom/kamcord/android/core/KC_H;)V

    invoke-direct {v0, v1}, Ljava/lang/Thread;-><init>(Ljava/lang/Runnable;)V

    invoke-virtual {v0}, Ljava/lang/Thread;->start()V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->N:Landroid/widget/ImageButton;

    new-instance v1, Lcom/kamcord/android/KC_N$KC_b;

    iget-object v2, p0, Lcom/kamcord/android/KC_N;->M:Landroid/app/Activity;

    invoke-direct {v1, p0, v2}, Lcom/kamcord/android/KC_N$KC_b;-><init>(Lcom/kamcord/android/KC_N;Landroid/app/Activity;)V

    invoke-virtual {v0, v1}, Landroid/widget/ImageButton;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->N:Landroid/widget/ImageButton;

    new-instance v1, Lcom/kamcord/android/KC_N$12;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_N$12;-><init>(Lcom/kamcord/android/KC_N;)V

    invoke-virtual {v0, v1}, Landroid/widget/ImageButton;->setOnTouchListener(Landroid/view/View$OnTouchListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->O:Landroid/widget/Button;

    new-instance v1, Lcom/kamcord/android/KC_N$KC_a;

    invoke-direct {v1, p0, p0, p1}, Lcom/kamcord/android/KC_N$KC_a;-><init>(Lcom/kamcord/android/KC_N;Lcom/kamcord/android/KC_N;Lcom/kamcord/android/core/KC_H;)V

    invoke-virtual {v0, v1}, Landroid/widget/Button;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->O:Landroid/widget/Button;

    const-string v1, "string"

    const-string v2, "kamcordShare"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/Button;->setText(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->O:Landroid/widget/Button;

    const/4 v1, 0x1

    invoke-virtual {v0, v1}, Landroid/widget/Button;->setEnabled(Z)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->N:Landroid/widget/ImageButton;

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Landroid/widget/ImageButton;->setVisibility(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->W:Landroid/widget/ImageView;

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Landroid/widget/ImageView;->setVisibility(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->O:Landroid/widget/Button;

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Landroid/widget/Button;->setVisibility(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->Z:Landroid/widget/ProgressBar;

    const/16 v1, 0x8

    invoke-virtual {v0, v1}, Landroid/widget/ProgressBar;->setVisibility(I)V
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    monitor-exit p0

    return-void

    :catchall_0
    move-exception v0

    monitor-exit p0

    throw v0
.end method

.method public final a(Ljava/lang/String;)V
    .locals 0

    return-void
.end method

.method public final a(Ljava/lang/String;F)V
    .locals 0

    return-void
.end method

.method public final a(Ljava/lang/String;I)V
    .locals 0

    return-void
.end method

.method public final a(Ljava/lang/String;Ljava/lang/String;)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_H;->a:Ljava/lang/String;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_H;->a:Ljava/lang/String;

    invoke-virtual {v0, p1}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-nez v0, :cond_1

    :cond_0
    :goto_0
    return-void

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iput-object p2, v0, Lcom/kamcord/android/core/KC_H;->i:Ljava/lang/String;

    goto :goto_0
.end method

.method public final a(Ljava/lang/String;Z)V
    .locals 2

    invoke-virtual {p0}, Lcom/kamcord/android/KC_N;->h()La/a/a/a/KC_f;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/KC_N$5;

    invoke-direct {v1, p0, p2, p1}, Lcom/kamcord/android/KC_N$5;-><init>(Lcom/kamcord/android/KC_N;ZLjava/lang/String;)V

    invoke-virtual {v0, v1}, La/a/a/a/KC_f;->runOnUiThread(Ljava/lang/Runnable;)V

    return-void
.end method

.method public final a(Ljava/util/HashSet;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V
    .locals 3
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Ljava/util/HashSet",
            "<",
            "Ljava/lang/String;",
            ">;",
            "Ljava/lang/String;",
            "Ljava/lang/String;",
            "Ljava/lang/String;",
            ")V"
        }
    .end annotation

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_H;->a:Ljava/lang/String;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_H;->a:Ljava/lang/String;

    invoke-virtual {v0, p2}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-nez v0, :cond_1

    :cond_0
    return-void

    :cond_1
    const-string v0, "line"

    invoke-virtual {p1, v0}, Ljava/util/HashSet;->contains(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_2

    const-string v0, "email"

    invoke-virtual {p1, v0}, Ljava/util/HashSet;->contains(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_2

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->Y:Ljava/util/HashMap;

    const-string v1, "line"

    invoke-virtual {v0, v1}, Ljava/util/HashMap;->containsKey(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_2

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->Y:Ljava/util/HashMap;

    const-string v1, "email"

    invoke-virtual {v0, v1}, Ljava/util/HashMap;->containsKey(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_2

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/KC_N;->ae:Z

    iput-object p2, p0, Lcom/kamcord/android/KC_N;->ag:Ljava/lang/String;

    iput-object p3, p0, Lcom/kamcord/android/KC_N;->ah:Ljava/lang/String;

    iput-object p4, p0, Lcom/kamcord/android/KC_N;->ai:Ljava/lang/String;

    :cond_2
    invoke-virtual {p1}, Ljava/util/HashSet;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :cond_3
    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/lang/String;

    iget-boolean v2, p0, Lcom/kamcord/android/KC_N;->ae:Z

    if-eqz v2, :cond_4

    const-string v2, "line"

    invoke-virtual {v0, v2}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v2

    if-nez v2, :cond_3

    :cond_4
    iget-object v2, p0, Lcom/kamcord/android/KC_N;->Y:Ljava/util/HashMap;

    invoke-virtual {v2, v0}, Ljava/util/HashMap;->containsKey(Ljava/lang/Object;)Z

    move-result v2

    if-eqz v2, :cond_3

    iget-object v2, p0, Lcom/kamcord/android/KC_N;->Y:Ljava/util/HashMap;

    invoke-virtual {v2, v0}, Ljava/util/HashMap;->get(Ljava/lang/Object;)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/b/KC_e;

    invoke-virtual {v0, p0, p3, p4}, Lcom/kamcord/android/b/KC_e;->a(La/a/a/a/KC_e;Ljava/lang/String;Ljava/lang/String;)V

    goto :goto_0
.end method

.method public final a_(Ljava/lang/String;)V
    .locals 0

    return-void
.end method

.method public final a_(Ljava/lang/String;Z)V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_H;->a:Ljava/lang/String;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_H;->a:Ljava/lang/String;

    invoke-virtual {v0, p1}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-nez v0, :cond_1

    :cond_0
    :goto_0
    return-void

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iput-boolean p2, v0, Lcom/kamcord/android/core/KC_H;->e:Z

    invoke-virtual {p0}, Lcom/kamcord/android/KC_N;->h()La/a/a/a/KC_f;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/KC_N$3;

    invoke-direct {v1, p0, p2}, Lcom/kamcord/android/KC_N$3;-><init>(Lcom/kamcord/android/KC_N;Z)V

    invoke-virtual {v0, v1}, La/a/a/a/KC_f;->runOnUiThread(Ljava/lang/Runnable;)V

    goto :goto_0
.end method

.method final b()V
    .locals 4

    const/16 v3, 0x8

    const/4 v1, 0x4

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->N:Landroid/widget/ImageButton;

    invoke-virtual {v0, v1}, Landroid/widget/ImageButton;->setVisibility(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->W:Landroid/widget/ImageView;

    invoke-virtual {v0, v3}, Landroid/widget/ImageView;->setVisibility(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->P:Landroid/widget/EditText;

    invoke-virtual {v0, v1}, Landroid/widget/EditText;->setVisibility(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->O:Landroid/widget/Button;

    const-string v1, "string"

    const-string v2, "kamcordNoVideoAvailable"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/Button;->setText(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->O:Landroid/widget/Button;

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Landroid/widget/Button;->setEnabled(Z)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->Z:Landroid/widget/ProgressBar;

    invoke-virtual {v0, v3}, Landroid/widget/ProgressBar;->setVisibility(I)V

    return-void
.end method

.method public final b(Ljava/lang/String;)V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_H;->a:Ljava/lang/String;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_H;->a:Ljava/lang/String;

    invoke-virtual {v0, p1}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-nez v0, :cond_1

    :cond_0
    :goto_0
    return-void

    :cond_1
    invoke-virtual {p0}, Lcom/kamcord/android/KC_N;->h()La/a/a/a/KC_f;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/KC_N$4;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_N$4;-><init>(Lcom/kamcord/android/KC_N;)V

    invoke-virtual {v0, v1}, La/a/a/a/KC_f;->runOnUiThread(Ljava/lang/Runnable;)V

    goto :goto_0
.end method

.method public final b_()V
    .locals 0

    invoke-direct {p0}, Lcom/kamcord/android/KC_N;->C()V

    return-void
.end method

.method public final d(Landroid/os/Bundle;)V
    .locals 4

    const-string v0, "video_share_local_id"

    iget-object v1, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    iget-object v1, v1, Lcom/kamcord/android/core/KC_H;->a:Ljava/lang/String;

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putString(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "video_share_directory_path"

    iget-object v1, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    iget-object v1, v1, Lcom/kamcord/android/core/KC_H;->b:Ljava/lang/String;

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putString(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "video_share_ready"

    iget-object v1, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    iget-boolean v1, v1, Lcom/kamcord/android/core/KC_H;->c:Z

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putBoolean(Ljava/lang/String;Z)V

    const-string v0, "video_share_muxer_will_finish"

    iget-object v1, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    iget-boolean v1, v1, Lcom/kamcord/android/core/KC_H;->d:Z

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putBoolean(Ljava/lang/String;Z)V

    const-string v0, "video_share_successfully_uploaded"

    iget-object v1, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    iget-boolean v1, v1, Lcom/kamcord/android/core/KC_H;->e:Z

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putBoolean(Ljava/lang/String;Z)V

    const-string v0, "video_share_title"

    iget-object v1, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    iget-object v1, v1, Lcom/kamcord/android/core/KC_H;->f:Ljava/lang/String;

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putString(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "video_share_metadata"

    iget-object v1, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    iget-object v1, v1, Lcom/kamcord/android/core/KC_H;->g:Lorg/json/JSONObject;

    invoke-virtual {v1}, Lorg/json/JSONObject;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putString(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "video_share_duration"

    iget-object v1, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    iget-wide v2, v1, Lcom/kamcord/android/core/KC_H;->h:D

    invoke-virtual {p1, v0, v2, v3}, Landroid/os/Bundle;->putDouble(Ljava/lang/String;D)V

    const-string v0, "video_share_server_id"

    iget-object v1, p0, Lcom/kamcord/android/KC_N;->aa:Lcom/kamcord/android/core/KC_H;

    iget-object v1, v1, Lcom/kamcord/android/core/KC_H;->i:Ljava/lang/String;

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putString(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "video_local_id"

    iget-object v1, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-object v1, v1, Lcom/kamcord/android/core/KC_H;->a:Ljava/lang/String;

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putString(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "video_directory_path"

    iget-object v1, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-object v1, v1, Lcom/kamcord/android/core/KC_H;->b:Ljava/lang/String;

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putString(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "video_ready"

    iget-object v1, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-boolean v1, v1, Lcom/kamcord/android/core/KC_H;->c:Z

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putBoolean(Ljava/lang/String;Z)V

    const-string v0, "video_muxer_will_finish"

    iget-object v1, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-boolean v1, v1, Lcom/kamcord/android/core/KC_H;->d:Z

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putBoolean(Ljava/lang/String;Z)V

    const-string v0, "video_successfully_uploaded"

    iget-object v1, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-boolean v1, v1, Lcom/kamcord/android/core/KC_H;->e:Z

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putBoolean(Ljava/lang/String;Z)V

    const-string v0, "video_title"

    iget-object v1, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-object v1, v1, Lcom/kamcord/android/core/KC_H;->f:Ljava/lang/String;

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putString(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "video_metadata"

    iget-object v1, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-object v1, v1, Lcom/kamcord/android/core/KC_H;->g:Lorg/json/JSONObject;

    invoke-virtual {v1}, Lorg/json/JSONObject;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putString(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "video_duration"

    iget-object v1, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-wide v2, v1, Lcom/kamcord/android/core/KC_H;->h:D

    invoke-virtual {p1, v0, v2, v3}, Landroid/os/Bundle;->putDouble(Ljava/lang/String;D)V

    const-string v0, "video_server_id"

    iget-object v1, p0, Lcom/kamcord/android/KC_N;->U:Lcom/kamcord/android/core/KC_H;

    iget-object v1, v1, Lcom/kamcord/android/core/KC_H;->i:Ljava/lang/String;

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putString(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "vo_dev_enabled"

    iget-boolean v1, p0, Lcom/kamcord/android/KC_N;->ab:Z

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putBoolean(Ljava/lang/String;Z)V

    const-string v0, "vo_player_enabled"

    iget-boolean v1, p0, Lcom/kamcord/android/KC_N;->ac:Z

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putBoolean(Ljava/lang/String;Z)V

    const-string v0, "vo_track_exists"

    iget-boolean v1, p0, Lcom/kamcord/android/KC_N;->ad:Z

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putBoolean(Ljava/lang/String;Z)V

    const-string v0, "line_and_email"

    iget-boolean v1, p0, Lcom/kamcord/android/KC_N;->ae:Z

    invoke-virtual {p1, v0, v1}, Landroid/os/Bundle;->putBoolean(Ljava/lang/String;Z)V

    invoke-super {p0, p1}, La/a/a/a/KC_e;->d(Landroid/os/Bundle;)V

    return-void
.end method

.method public final localVideoFailed(Lcom/kamcord/android/core/KC_H;)V
    .locals 2

    invoke-virtual {p0}, Lcom/kamcord/android/KC_N;->h()La/a/a/a/KC_f;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/KC_N$2;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_N$2;-><init>(Lcom/kamcord/android/KC_N;)V

    invoke-virtual {v0, v1}, La/a/a/a/KC_f;->runOnUiThread(Ljava/lang/Runnable;)V

    return-void
.end method

.method public final localVideoReady(Lcom/kamcord/android/core/KC_H;)V
    .locals 2

    invoke-virtual {p0}, Lcom/kamcord/android/KC_N;->h()La/a/a/a/KC_f;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/KC_N$13;

    invoke-direct {v1, p0, p1}, Lcom/kamcord/android/KC_N$13;-><init>(Lcom/kamcord/android/KC_N;Lcom/kamcord/android/core/KC_H;)V

    invoke-virtual {v0, v1}, La/a/a/a/KC_f;->runOnUiThread(Ljava/lang/Runnable;)V

    return-void
.end method

.method public final o()V
    .locals 0

    invoke-super {p0}, La/a/a/a/KC_e;->o()V

    invoke-direct {p0}, Lcom/kamcord/android/KC_N;->B()V

    return-void
.end method

.method public final p()V
    .locals 0

    invoke-super {p0}, La/a/a/a/KC_e;->p()V

    return-void
.end method

.method public final q()V
    .locals 1

    invoke-super {p0}, La/a/a/a/KC_e;->q()V

    invoke-static {p0}, Lcom/kamcord/android/KC_Y;->b(Lcom/kamcord/android/KC_X;)Z

    invoke-static {p0}, Lcom/kamcord/android/KC_H;->b(Lcom/kamcord/android/KC_H$KC_a;)Z

    invoke-static {p0}, Lcom/kamcord/android/core/KC_S;->b(Lcom/kamcord/android/VideoStatusListener;)Z

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getAuthCenter()Lcom/kamcord/android/KC_e;

    move-result-object v0

    invoke-virtual {v0, p0}, Lcom/kamcord/android/KC_e;->b(Lcom/kamcord/android/AuthListener;)V

    return-void
.end method
