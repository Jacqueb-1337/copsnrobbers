.class public Lcom/kamcord/android/KC_w;
.super Landroid/widget/FrameLayout;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/KC_w$KC_a;,
        Lcom/kamcord/android/KC_w$KC_b;
    }
.end annotation


# instance fields
.field private A:Landroid/os/Handler;

.field private B:Ljava/lang/String;

.field private C:Landroid/view/View$OnClickListener;

.field private D:I

.field private E:Z

.field private F:Landroid/widget/SeekBar$OnSeekBarChangeListener;

.field private a:Lcom/kamcord/android/KC_E;

.field private b:Landroid/content/Context;

.field private c:Landroid/view/ViewGroup;

.field private d:Landroid/view/View;

.field private e:Landroid/widget/ProgressBar;

.field private f:Landroid/widget/TextView;

.field private g:Landroid/widget/TextView;

.field private h:Landroid/widget/TextView;

.field private i:Z

.field private j:Z

.field private k:Z

.field private l:Z

.field private m:Landroid/view/View$OnClickListener;

.field private n:Landroid/view/View$OnClickListener;

.field private o:Landroid/view/View$OnClickListener;

.field private p:Ljava/lang/StringBuilder;

.field private q:Ljava/util/Formatter;

.field private r:Landroid/widget/ImageView;

.field private s:Landroid/widget/LinearLayout;

.field private t:Landroid/widget/ImageView;

.field private u:Landroid/widget/LinearLayout;

.field private v:Landroid/widget/ImageView;

.field private w:Landroid/widget/LinearLayout;

.field private x:Landroid/widget/ImageView;

.field private y:Landroid/widget/LinearLayout;

.field private z:Z


# direct methods
.method public constructor <init>(Landroid/content/Context;)V
    .locals 1

    invoke-direct {p0, p1}, Landroid/widget/FrameLayout;-><init>(Landroid/content/Context;)V

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/KC_w;->l:Z

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/KC_w;->z:Z

    new-instance v0, Lcom/kamcord/android/KC_w$KC_b;

    invoke-direct {v0, p0}, Lcom/kamcord/android/KC_w$KC_b;-><init>(Lcom/kamcord/android/KC_w;)V

    iput-object v0, p0, Lcom/kamcord/android/KC_w;->A:Landroid/os/Handler;

    const-string v0, ""

    iput-object v0, p0, Lcom/kamcord/android/KC_w;->B:Ljava/lang/String;

    new-instance v0, Lcom/kamcord/android/KC_w$6;

    invoke-direct {v0, p0}, Lcom/kamcord/android/KC_w$6;-><init>(Lcom/kamcord/android/KC_w;)V

    iput-object v0, p0, Lcom/kamcord/android/KC_w;->C:Landroid/view/View$OnClickListener;

    new-instance v0, Lcom/kamcord/android/KC_w$7;

    invoke-direct {v0, p0}, Lcom/kamcord/android/KC_w$7;-><init>(Lcom/kamcord/android/KC_w;)V

    iput-object v0, p0, Lcom/kamcord/android/KC_w;->F:Landroid/widget/SeekBar$OnSeekBarChangeListener;

    iput-object p1, p0, Lcom/kamcord/android/KC_w;->b:Landroid/content/Context;

    return-void
.end method

.method static synthetic a(Lcom/kamcord/android/KC_w;I)I
    .locals 0

    iput p1, p0, Lcom/kamcord/android/KC_w;->D:I

    return p1
.end method

.method private a(Landroid/view/View;)V
    .locals 4
    .annotation build Landroid/annotation/SuppressLint;
        value = {
            "ClickableViewAccessibility"
        }
    .end annotation

    const/16 v3, 0x8

    const-string v0, "id"

    const-string v1, "pauseImage"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {p1, v0}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/ImageView;

    iput-object v0, p0, Lcom/kamcord/android/KC_w;->r:Landroid/widget/ImageView;

    const-string v0, "id"

    const-string v1, "pauseButton"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {p1, v0}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/LinearLayout;

    iput-object v0, p0, Lcom/kamcord/android/KC_w;->s:Landroid/widget/LinearLayout;

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->s:Landroid/widget/LinearLayout;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->s:Landroid/widget/LinearLayout;

    invoke-virtual {v0}, Landroid/widget/LinearLayout;->requestFocus()Z

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->s:Landroid/widget/LinearLayout;

    iget-object v1, p0, Lcom/kamcord/android/KC_w;->C:Landroid/view/View$OnClickListener;

    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    :cond_0
    const-string v0, "id"

    const-string v1, "fullscreenImage"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {p1, v0}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/ImageView;

    iput-object v0, p0, Lcom/kamcord/android/KC_w;->x:Landroid/widget/ImageView;

    const-string v0, "id"

    const-string v1, "fullscreenButton"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {p1, v0}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/LinearLayout;

    iput-object v0, p0, Lcom/kamcord/android/KC_w;->y:Landroid/widget/LinearLayout;

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->y:Landroid/widget/LinearLayout;

    if-eqz v0, :cond_1

    iget-boolean v0, p0, Lcom/kamcord/android/KC_w;->l:Z

    if-nez v0, :cond_7

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->y:Landroid/widget/LinearLayout;

    invoke-virtual {v0, v3}, Landroid/widget/LinearLayout;->setVisibility(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->x:Landroid/widget/ImageView;

    invoke-virtual {v0, v3}, Landroid/widget/ImageView;->setVisibility(I)V

    :cond_1
    :goto_0
    const-string v0, "id"

    const-string v1, "nextImage"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {p1, v0}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/ImageView;

    iput-object v0, p0, Lcom/kamcord/android/KC_w;->v:Landroid/widget/ImageView;

    const-string v0, "id"

    const-string v1, "nextButton"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {p1, v0}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/LinearLayout;

    iput-object v0, p0, Lcom/kamcord/android/KC_w;->w:Landroid/widget/LinearLayout;

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->w:Landroid/widget/LinearLayout;

    if-eqz v0, :cond_2

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->v:Landroid/widget/ImageView;

    const-string v1, "drawable"

    const-string v2, "kamcord_player_next_icon"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/ImageView;->setImageResource(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->w:Landroid/widget/LinearLayout;

    invoke-virtual {v0, v3}, Landroid/widget/LinearLayout;->setVisibility(I)V

    :cond_2
    const-string v0, "id"

    const-string v1, "prevImage"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {p1, v0}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/ImageView;

    iput-object v0, p0, Lcom/kamcord/android/KC_w;->t:Landroid/widget/ImageView;

    const-string v0, "id"

    const-string v1, "prevButton"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {p1, v0}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/LinearLayout;

    iput-object v0, p0, Lcom/kamcord/android/KC_w;->u:Landroid/widget/LinearLayout;

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->u:Landroid/widget/LinearLayout;

    if-eqz v0, :cond_3

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->t:Landroid/widget/ImageView;

    const-string v1, "drawable"

    const-string v2, "kamcord_player_prev_icon"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/ImageView;->setImageResource(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->u:Landroid/widget/LinearLayout;

    invoke-virtual {v0, v3}, Landroid/widget/LinearLayout;->setVisibility(I)V

    :cond_3
    const-string v0, "id"

    const-string v1, "mediacontroller_progress"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {p1, v0}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/ProgressBar;

    iput-object v0, p0, Lcom/kamcord/android/KC_w;->e:Landroid/widget/ProgressBar;

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->e:Landroid/widget/ProgressBar;

    if-eqz v0, :cond_5

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->e:Landroid/widget/ProgressBar;

    instance-of v0, v0, Landroid/widget/SeekBar;

    if-eqz v0, :cond_4

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->e:Landroid/widget/ProgressBar;

    check-cast v0, Landroid/widget/SeekBar;

    invoke-virtual {v0}, Landroid/widget/SeekBar;->getThumb()Landroid/graphics/drawable/Drawable;

    move-result-object v1

    invoke-virtual {v1}, Landroid/graphics/drawable/Drawable;->getIntrinsicWidth()I

    move-result v1

    div-int/lit8 v1, v1, 0x2

    invoke-virtual {v0, v1}, Landroid/widget/SeekBar;->setThumbOffset(I)V

    iget-object v1, p0, Lcom/kamcord/android/KC_w;->F:Landroid/widget/SeekBar$OnSeekBarChangeListener;

    invoke-virtual {v0, v1}, Landroid/widget/SeekBar;->setOnSeekBarChangeListener(Landroid/widget/SeekBar$OnSeekBarChangeListener;)V

    :cond_4
    iget-object v0, p0, Lcom/kamcord/android/KC_w;->e:Landroid/widget/ProgressBar;

    const/16 v1, 0x3e8

    invoke-virtual {v0, v1}, Landroid/widget/ProgressBar;->setMax(I)V

    :cond_5
    const-string v0, "id"

    const-string v1, "time"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {p1, v0}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/TextView;

    iput-object v0, p0, Lcom/kamcord/android/KC_w;->g:Landroid/widget/TextView;

    const-string v0, "id"

    const-string v1, "time_current"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {p1, v0}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/TextView;

    iput-object v0, p0, Lcom/kamcord/android/KC_w;->h:Landroid/widget/TextView;

    new-instance v0, Ljava/lang/StringBuilder;

    invoke-direct {v0}, Ljava/lang/StringBuilder;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/KC_w;->p:Ljava/lang/StringBuilder;

    new-instance v0, Ljava/util/Formatter;

    iget-object v1, p0, Lcom/kamcord/android/KC_w;->p:Ljava/lang/StringBuilder;

    invoke-static {}, Ljava/util/Locale;->getDefault()Ljava/util/Locale;

    move-result-object v2

    invoke-direct {v0, v1, v2}, Ljava/util/Formatter;-><init>(Ljava/lang/Appendable;Ljava/util/Locale;)V

    iput-object v0, p0, Lcom/kamcord/android/KC_w;->q:Ljava/util/Formatter;

    const-string v0, "id"

    const-string v1, "title_text"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {p1, v0}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/TextView;

    iput-object v0, p0, Lcom/kamcord/android/KC_w;->f:Landroid/widget/TextView;

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->f:Landroid/widget/TextView;

    iget-object v1, p0, Lcom/kamcord/android/KC_w;->B:Ljava/lang/String;

    invoke-virtual {v0, v1}, Landroid/widget/TextView;->setText(Ljava/lang/CharSequence;)V

    iget-boolean v0, p0, Lcom/kamcord/android/KC_w;->i:Z

    if-nez v0, :cond_6

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->f:Landroid/widget/TextView;

    invoke-virtual {v0, v3}, Landroid/widget/TextView;->setVisibility(I)V

    :cond_6
    const-string v0, "id"

    const-string v1, "filler"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {p1, v0}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/RelativeLayout;

    new-instance v1, Lcom/kamcord/android/KC_w$1;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_w$1;-><init>(Lcom/kamcord/android/KC_w;)V

    invoke-virtual {v0, v1}, Landroid/widget/RelativeLayout;->setOnTouchListener(Landroid/view/View$OnTouchListener;)V

    new-instance v0, Lcom/kamcord/android/KC_w$2;

    invoke-direct {v0, p0}, Lcom/kamcord/android/KC_w$2;-><init>(Lcom/kamcord/android/KC_w;)V

    invoke-virtual {p1, v0}, Landroid/view/View;->setOnTouchListener(Landroid/view/View$OnTouchListener;)V

    const-string v0, "id"

    const-string v1, "title_bar"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {p1, v0}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/LinearLayout;

    new-instance v1, Lcom/kamcord/android/KC_w$3;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_w$3;-><init>(Lcom/kamcord/android/KC_w;)V

    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->setOnTouchListener(Landroid/view/View$OnTouchListener;)V

    const-string v0, "id"

    const-string v1, "control_bar"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    invoke-virtual {p1, v0}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/LinearLayout;

    new-instance v1, Lcom/kamcord/android/KC_w$4;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_w$4;-><init>(Lcom/kamcord/android/KC_w;)V

    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->setOnTouchListener(Landroid/view/View$OnTouchListener;)V

    return-void

    :cond_7
    iget-object v0, p0, Lcom/kamcord/android/KC_w;->y:Landroid/widget/LinearLayout;

    invoke-virtual {v0}, Landroid/widget/LinearLayout;->requestFocus()Z

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->y:Landroid/widget/LinearLayout;

    iget-object v1, p0, Lcom/kamcord/android/KC_w;->o:Landroid/view/View$OnClickListener;

    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    goto/16 :goto_0
.end method

.method static synthetic a(Lcom/kamcord/android/KC_w;)V
    .locals 2

    iget-boolean v0, p0, Lcom/kamcord/android/KC_w;->j:Z

    if-nez v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->c:Landroid/view/ViewGroup;

    invoke-virtual {v0, p0}, Landroid/view/ViewGroup;->indexOfChild(Landroid/view/View;)I

    move-result v0

    const/4 v1, -0x1

    if-eq v0, v1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->c:Landroid/view/ViewGroup;

    invoke-virtual {v0, p0}, Landroid/view/ViewGroup;->removeView(Landroid/view/View;)V

    :cond_0
    return-void
.end method

.method static synthetic a(Lcom/kamcord/android/KC_w;Z)Z
    .locals 0

    iput-boolean p1, p0, Lcom/kamcord/android/KC_w;->k:Z

    return p1
.end method

.method private b(I)Ljava/lang/String;
    .locals 9

    const/4 v8, 0x2

    const/4 v7, 0x1

    const/4 v6, 0x0

    div-int/lit16 v0, p1, 0x3e8

    rem-int/lit8 v1, v0, 0x3c

    div-int/lit8 v2, v0, 0x3c

    rem-int/lit8 v2, v2, 0x3c

    div-int/lit16 v0, v0, 0xe10

    iget-object v3, p0, Lcom/kamcord/android/KC_w;->p:Ljava/lang/StringBuilder;

    invoke-virtual {v3, v6}, Ljava/lang/StringBuilder;->setLength(I)V

    if-lez v0, :cond_0

    iget-object v3, p0, Lcom/kamcord/android/KC_w;->q:Ljava/util/Formatter;

    const-string v4, "%d:%02d:%02d"

    const/4 v5, 0x3

    new-array v5, v5, [Ljava/lang/Object;

    invoke-static {v0}, Ljava/lang/Integer;->valueOf(I)Ljava/lang/Integer;

    move-result-object v0

    aput-object v0, v5, v6

    invoke-static {v2}, Ljava/lang/Integer;->valueOf(I)Ljava/lang/Integer;

    move-result-object v0

    aput-object v0, v5, v7

    invoke-static {v1}, Ljava/lang/Integer;->valueOf(I)Ljava/lang/Integer;

    move-result-object v0

    aput-object v0, v5, v8

    invoke-virtual {v3, v4, v5}, Ljava/util/Formatter;->format(Ljava/lang/String;[Ljava/lang/Object;)Ljava/util/Formatter;

    move-result-object v0

    invoke-virtual {v0}, Ljava/util/Formatter;->toString()Ljava/lang/String;

    move-result-object v0

    :goto_0
    return-object v0

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_w;->q:Ljava/util/Formatter;

    const-string v3, "%02d:%02d"

    new-array v4, v8, [Ljava/lang/Object;

    invoke-static {v2}, Ljava/lang/Integer;->valueOf(I)Ljava/lang/Integer;

    move-result-object v2

    aput-object v2, v4, v6

    invoke-static {v1}, Ljava/lang/Integer;->valueOf(I)Ljava/lang/Integer;

    move-result-object v1

    aput-object v1, v4, v7

    invoke-virtual {v0, v3, v4}, Ljava/util/Formatter;->format(Ljava/lang/String;[Ljava/lang/Object;)Ljava/util/Formatter;

    move-result-object v0

    invoke-virtual {v0}, Ljava/util/Formatter;->toString()Ljava/lang/String;

    move-result-object v0

    goto :goto_0
.end method

.method static synthetic b(Lcom/kamcord/android/KC_w;I)Ljava/lang/String;
    .locals 1

    invoke-direct {p0, p1}, Lcom/kamcord/android/KC_w;->b(I)Ljava/lang/String;

    move-result-object v0

    return-object v0
.end method

.method static synthetic b(Lcom/kamcord/android/KC_w;)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->a:Lcom/kamcord/android/KC_E;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->a:Lcom/kamcord/android/KC_E;

    invoke-interface {v0}, Lcom/kamcord/android/KC_E;->isPlaying()Z

    move-result v0

    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->a:Lcom/kamcord/android/KC_E;

    invoke-interface {v0}, Lcom/kamcord/android/KC_E;->pause()V

    :goto_0
    invoke-virtual {p0}, Lcom/kamcord/android/KC_w;->g()V

    :cond_0
    return-void

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_w;->a:Lcom/kamcord/android/KC_E;

    invoke-interface {v0}, Lcom/kamcord/android/KC_E;->start()V

    goto :goto_0
.end method

.method static synthetic b(Lcom/kamcord/android/KC_w;Z)Z
    .locals 0

    iput-boolean p1, p0, Lcom/kamcord/android/KC_w;->E:Z

    return p1
.end method

.method static synthetic c(Lcom/kamcord/android/KC_w;)Lcom/kamcord/android/KC_E;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->a:Lcom/kamcord/android/KC_E;

    return-object v0
.end method

.method static synthetic d(Lcom/kamcord/android/KC_w;)Z
    .locals 1

    iget-boolean v0, p0, Lcom/kamcord/android/KC_w;->E:Z

    return v0
.end method

.method static synthetic e(Lcom/kamcord/android/KC_w;)Landroid/os/Handler;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->A:Landroid/os/Handler;

    return-object v0
.end method

.method static synthetic f(Lcom/kamcord/android/KC_w;)Landroid/widget/TextView;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->h:Landroid/widget/TextView;

    return-object v0
.end method

.method static synthetic g(Lcom/kamcord/android/KC_w;)I
    .locals 1

    iget v0, p0, Lcom/kamcord/android/KC_w;->D:I

    return v0
.end method

.method static synthetic h(Lcom/kamcord/android/KC_w;)V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->a:Lcom/kamcord/android/KC_E;

    if-eqz v0, :cond_0

    iget-boolean v0, p0, Lcom/kamcord/android/KC_w;->z:Z

    if-nez v0, :cond_1

    const/4 v0, 0x1

    :goto_0
    iput-boolean v0, p0, Lcom/kamcord/android/KC_w;->z:Z

    invoke-direct {p0}, Lcom/kamcord/android/KC_w;->i()V

    :cond_0
    return-void

    :cond_1
    const/4 v0, 0x0

    goto :goto_0
.end method

.method static synthetic i(Lcom/kamcord/android/KC_w;)Landroid/view/View$OnClickListener;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->o:Landroid/view/View$OnClickListener;

    return-object v0
.end method

.method private i()V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->d:Landroid/view/View;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->y:Landroid/widget/LinearLayout;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->a:Lcom/kamcord/android/KC_E;

    if-nez v0, :cond_1

    :cond_0
    :goto_0
    return-void

    :cond_1
    iget-boolean v0, p0, Lcom/kamcord/android/KC_w;->z:Z

    if-eqz v0, :cond_2

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->x:Landroid/widget/ImageView;

    const-string v1, "drawable"

    const-string v2, "kamcord_player_unfullscreen_icon"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/ImageView;->setImageResource(I)V

    :goto_1
    iget-boolean v0, p0, Lcom/kamcord/android/KC_w;->l:Z

    if-nez v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->y:Landroid/widget/LinearLayout;

    const/16 v1, 0x8

    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->setVisibility(I)V

    goto :goto_0

    :cond_2
    iget-object v0, p0, Lcom/kamcord/android/KC_w;->x:Landroid/widget/ImageView;

    const-string v1, "drawable"

    const-string v2, "kamcord_player_fullscreen_icon"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/ImageView;->setImageResource(I)V

    goto :goto_1
.end method

.method static synthetic j(Lcom/kamcord/android/KC_w;)Z
    .locals 1

    iget-boolean v0, p0, Lcom/kamcord/android/KC_w;->k:Z

    return v0
.end method

.method static synthetic k(Lcom/kamcord/android/KC_w;)Z
    .locals 1

    iget-boolean v0, p0, Lcom/kamcord/android/KC_w;->j:Z

    return v0
.end method


# virtual methods
.method public final a()V
    .locals 1

    const/16 v0, 0xfa0

    invoke-virtual {p0, v0}, Lcom/kamcord/android/KC_w;->a(I)V

    return-void
.end method

.method public final a(I)V
    .locals 4

    const/4 v3, 0x1

    const/4 v1, -0x1

    iget-boolean v0, p0, Lcom/kamcord/android/KC_w;->j:Z

    if-nez v0, :cond_2

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->c:Landroid/view/ViewGroup;

    if-eqz v0, :cond_2

    invoke-virtual {p0}, Lcom/kamcord/android/KC_w;->e()I

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->s:Landroid/widget/LinearLayout;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->s:Landroid/widget/LinearLayout;

    invoke-virtual {v0}, Landroid/widget/LinearLayout;->requestFocus()Z

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_w;->c:Landroid/view/ViewGroup;

    invoke-virtual {v0, p0}, Landroid/view/ViewGroup;->indexOfChild(Landroid/view/View;)I

    move-result v0

    if-ne v0, v1, :cond_1

    new-instance v0, Landroid/widget/FrameLayout$LayoutParams;

    invoke-direct {v0, v1, v1}, Landroid/widget/FrameLayout$LayoutParams;-><init>(II)V

    iget-object v1, p0, Lcom/kamcord/android/KC_w;->c:Landroid/view/ViewGroup;

    invoke-virtual {v1, p0, v0}, Landroid/view/ViewGroup;->addView(Landroid/view/View;Landroid/view/ViewGroup$LayoutParams;)V

    :cond_1
    new-instance v0, Lcom/kamcord/android/KC_w$KC_a;

    const/4 v1, 0x0

    const/high16 v2, 0x3f800000    # 1.0f

    invoke-direct {v0, p0, v1, v2}, Lcom/kamcord/android/KC_w$KC_a;-><init>(Lcom/kamcord/android/KC_w;FF)V

    iget-object v1, p0, Lcom/kamcord/android/KC_w;->d:Landroid/view/View;

    invoke-virtual {v1}, Landroid/view/View;->clearAnimation()V

    iget-object v1, p0, Lcom/kamcord/android/KC_w;->d:Landroid/view/View;

    invoke-virtual {v1, v0}, Landroid/view/View;->startAnimation(Landroid/view/animation/Animation;)V

    iput-boolean v3, p0, Lcom/kamcord/android/KC_w;->j:Z

    :cond_2
    invoke-virtual {p0}, Lcom/kamcord/android/KC_w;->g()V

    invoke-direct {p0}, Lcom/kamcord/android/KC_w;->i()V

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->A:Landroid/os/Handler;

    const/4 v1, 0x2

    invoke-virtual {v0, v1}, Landroid/os/Handler;->sendEmptyMessage(I)Z

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->A:Landroid/os/Handler;

    invoke-virtual {v0, v3}, Landroid/os/Handler;->obtainMessage(I)Landroid/os/Message;

    move-result-object v0

    if-lez p1, :cond_3

    iget-object v1, p0, Lcom/kamcord/android/KC_w;->A:Landroid/os/Handler;

    invoke-virtual {v1, v3}, Landroid/os/Handler;->removeMessages(I)V

    iget-object v1, p0, Lcom/kamcord/android/KC_w;->A:Landroid/os/Handler;

    int-to-long v2, p1

    invoke-virtual {v1, v0, v2, v3}, Landroid/os/Handler;->sendMessageDelayed(Landroid/os/Message;J)Z

    :cond_3
    return-void
.end method

.method public final a(Landroid/view/View$OnClickListener;Landroid/view/View$OnClickListener;Landroid/view/View$OnClickListener;)V
    .locals 5

    const/4 v1, 0x1

    const/4 v2, 0x0

    iput-object p2, p0, Lcom/kamcord/android/KC_w;->m:Landroid/view/View$OnClickListener;

    iput-object p1, p0, Lcom/kamcord/android/KC_w;->n:Landroid/view/View$OnClickListener;

    iput-object p3, p0, Lcom/kamcord/android/KC_w;->o:Landroid/view/View$OnClickListener;

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->w:Landroid/widget/LinearLayout;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->w:Landroid/widget/LinearLayout;

    iget-object v3, p0, Lcom/kamcord/android/KC_w;->m:Landroid/view/View$OnClickListener;

    invoke-virtual {v0, v3}, Landroid/widget/LinearLayout;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    iget-object v3, p0, Lcom/kamcord/android/KC_w;->w:Landroid/widget/LinearLayout;

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->m:Landroid/view/View$OnClickListener;

    if-eqz v0, :cond_3

    move v0, v1

    :goto_0
    invoke-virtual {v3, v0}, Landroid/widget/LinearLayout;->setEnabled(Z)V

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->m:Landroid/view/View$OnClickListener;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->w:Landroid/widget/LinearLayout;

    invoke-virtual {v0, v2}, Landroid/widget/LinearLayout;->setVisibility(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->v:Landroid/widget/ImageView;

    const-string v3, "drawable"

    const-string v4, "kamcord_player_next_icon"

    invoke-static {v3, v4}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v3

    invoke-virtual {v0, v3}, Landroid/widget/ImageView;->setImageResource(I)V

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_w;->u:Landroid/widget/LinearLayout;

    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->u:Landroid/widget/LinearLayout;

    iget-object v3, p0, Lcom/kamcord/android/KC_w;->n:Landroid/view/View$OnClickListener;

    invoke-virtual {v0, v3}, Landroid/widget/LinearLayout;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    iget-object v3, p0, Lcom/kamcord/android/KC_w;->u:Landroid/widget/LinearLayout;

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->n:Landroid/view/View$OnClickListener;

    if-eqz v0, :cond_4

    move v0, v1

    :goto_1
    invoke-virtual {v3, v0}, Landroid/widget/LinearLayout;->setEnabled(Z)V

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->n:Landroid/view/View$OnClickListener;

    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->u:Landroid/widget/LinearLayout;

    invoke-virtual {v0, v2}, Landroid/widget/LinearLayout;->setVisibility(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->t:Landroid/widget/ImageView;

    const-string v3, "drawable"

    const-string v4, "kamcord_player_prev_icon"

    invoke-static {v3, v4}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v3

    invoke-virtual {v0, v3}, Landroid/widget/ImageView;->setImageResource(I)V

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_w;->y:Landroid/widget/LinearLayout;

    if-eqz v0, :cond_2

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->y:Landroid/widget/LinearLayout;

    new-instance v3, Lcom/kamcord/android/KC_w$8;

    invoke-direct {v3, p0}, Lcom/kamcord/android/KC_w$8;-><init>(Lcom/kamcord/android/KC_w;)V

    invoke-virtual {v0, v3}, Landroid/widget/LinearLayout;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->y:Landroid/widget/LinearLayout;

    iget-object v3, p0, Lcom/kamcord/android/KC_w;->o:Landroid/view/View$OnClickListener;

    if-eqz v3, :cond_5

    :goto_2
    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->setEnabled(Z)V

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->o:Landroid/view/View$OnClickListener;

    if-eqz v0, :cond_2

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->y:Landroid/widget/LinearLayout;

    invoke-virtual {v0, v2}, Landroid/widget/LinearLayout;->setVisibility(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->x:Landroid/widget/ImageView;

    const-string v1, "drawable"

    const-string v2, "kamcord_player_fullscreen_icon"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/ImageView;->setImageResource(I)V

    :cond_2
    return-void

    :cond_3
    move v0, v2

    goto :goto_0

    :cond_4
    move v0, v2

    goto :goto_1

    :cond_5
    move v1, v2

    goto :goto_2
.end method

.method public final a(Landroid/widget/FrameLayout;)V
    .locals 4

    const/4 v0, -0x1

    iput-object p1, p0, Lcom/kamcord/android/KC_w;->c:Landroid/view/ViewGroup;

    new-instance v1, Landroid/widget/FrameLayout$LayoutParams;

    invoke-direct {v1, v0, v0}, Landroid/widget/FrameLayout$LayoutParams;-><init>(II)V

    invoke-virtual {p0}, Lcom/kamcord/android/KC_w;->removeAllViews()V

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->b:Landroid/content/Context;

    const-string v2, "layout_inflater"

    invoke-virtual {v0, v2}, Landroid/content/Context;->getSystemService(Ljava/lang/String;)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Landroid/view/LayoutInflater;

    const-string v2, "layout"

    const-string v3, "z_kamcord_media_controller"

    invoke-static {v2, v3}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    const/4 v3, 0x0

    invoke-virtual {v0, v2, v3}, Landroid/view/LayoutInflater;->inflate(ILandroid/view/ViewGroup;)Landroid/view/View;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_w;->d:Landroid/view/View;

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->d:Landroid/view/View;

    invoke-direct {p0, v0}, Lcom/kamcord/android/KC_w;->a(Landroid/view/View;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->d:Landroid/view/View;

    invoke-virtual {p0, v0, v1}, Lcom/kamcord/android/KC_w;->addView(Landroid/view/View;Landroid/view/ViewGroup$LayoutParams;)V

    return-void
.end method

.method public final a(Lcom/kamcord/android/KC_E;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_w;->a:Lcom/kamcord/android/KC_E;

    invoke-virtual {p0}, Lcom/kamcord/android/KC_w;->g()V

    invoke-direct {p0}, Lcom/kamcord/android/KC_w;->i()V

    return-void
.end method

.method public final a(Ljava/lang/String;)V
    .locals 2

    iput-object p1, p0, Lcom/kamcord/android/KC_w;->B:Ljava/lang/String;

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->f:Landroid/widget/TextView;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->f:Landroid/widget/TextView;

    iget-object v1, p0, Lcom/kamcord/android/KC_w;->B:Ljava/lang/String;

    invoke-virtual {v0, v1}, Landroid/widget/TextView;->setText(Ljava/lang/CharSequence;)V

    :cond_0
    return-void
.end method

.method public final a(Z)V
    .locals 2

    iput-boolean p1, p0, Lcom/kamcord/android/KC_w;->i:Z

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->f:Landroid/widget/TextView;

    if-eqz v0, :cond_0

    iget-boolean v0, p0, Lcom/kamcord/android/KC_w;->i:Z

    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->f:Landroid/widget/TextView;

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Landroid/widget/TextView;->setVisibility(I)V

    :cond_0
    :goto_0
    return-void

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_w;->f:Landroid/widget/TextView;

    const/16 v1, 0x8

    invoke-virtual {v0, v1}, Landroid/widget/TextView;->setVisibility(I)V

    goto :goto_0
.end method

.method public final b()Z
    .locals 1

    iget-boolean v0, p0, Lcom/kamcord/android/KC_w;->j:Z

    return v0
.end method

.method public final c()Z
    .locals 1

    iget-boolean v0, p0, Lcom/kamcord/android/KC_w;->z:Z

    return v0
.end method

.method public final d()V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->c:Landroid/view/ViewGroup;

    if-nez v0, :cond_0

    :goto_0
    return-void

    :cond_0
    :try_start_0
    new-instance v0, Lcom/kamcord/android/KC_w$KC_a;

    const/high16 v1, 0x3f800000    # 1.0f

    const/4 v2, 0x0

    invoke-direct {v0, p0, v1, v2}, Lcom/kamcord/android/KC_w$KC_a;-><init>(Lcom/kamcord/android/KC_w;FF)V

    new-instance v1, Lcom/kamcord/android/KC_w$5;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_w$5;-><init>(Lcom/kamcord/android/KC_w;)V

    invoke-virtual {v0, v1}, Landroid/view/animation/Animation;->setAnimationListener(Landroid/view/animation/Animation$AnimationListener;)V

    iget-object v1, p0, Lcom/kamcord/android/KC_w;->d:Landroid/view/View;

    invoke-virtual {v1}, Landroid/view/View;->clearAnimation()V

    iget-object v1, p0, Lcom/kamcord/android/KC_w;->d:Landroid/view/View;

    invoke-virtual {v1, v0}, Landroid/view/View;->startAnimation(Landroid/view/animation/Animation;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->A:Landroid/os/Handler;

    const/4 v1, 0x2

    invoke-virtual {v0, v1}, Landroid/os/Handler;->removeMessages(I)V
    :try_end_0
    .catch Ljava/lang/IllegalArgumentException; {:try_start_0 .. :try_end_0} :catch_0

    :goto_1
    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/KC_w;->j:Z

    goto :goto_0

    :catch_0
    move-exception v0

    const-string v0, "MediaController"

    const-string v1, "already removed"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord$KC_a;->b(Ljava/lang/String;Ljava/lang/String;)I

    goto :goto_1
.end method

.method public final e()I
    .locals 6

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->a:Lcom/kamcord/android/KC_E;

    if-eqz v0, :cond_0

    iget-boolean v0, p0, Lcom/kamcord/android/KC_w;->k:Z

    if-eqz v0, :cond_2

    :cond_0
    const/4 v0, 0x0

    :cond_1
    :goto_0
    return v0

    :cond_2
    iget-object v0, p0, Lcom/kamcord/android/KC_w;->a:Lcom/kamcord/android/KC_E;

    invoke-interface {v0}, Lcom/kamcord/android/KC_E;->getCurrentPosition()I

    move-result v0

    iget-object v1, p0, Lcom/kamcord/android/KC_w;->a:Lcom/kamcord/android/KC_E;

    invoke-interface {v1}, Lcom/kamcord/android/KC_E;->getDuration()I

    move-result v1

    iget-object v2, p0, Lcom/kamcord/android/KC_w;->a:Lcom/kamcord/android/KC_E;

    invoke-interface {v2}, Lcom/kamcord/android/KC_E;->a()Z

    move-result v2

    if-eqz v2, :cond_3

    move v0, v1

    :cond_3
    iget-object v2, p0, Lcom/kamcord/android/KC_w;->e:Landroid/widget/ProgressBar;

    if-eqz v2, :cond_5

    if-lez v1, :cond_4

    const-wide/16 v2, 0x3e8

    int-to-long v4, v0

    mul-long/2addr v2, v4

    int-to-long v4, v1

    div-long/2addr v2, v4

    iget-object v4, p0, Lcom/kamcord/android/KC_w;->e:Landroid/widget/ProgressBar;

    long-to-int v2, v2

    invoke-virtual {v4, v2}, Landroid/widget/ProgressBar;->setProgress(I)V

    :cond_4
    iget-object v2, p0, Lcom/kamcord/android/KC_w;->a:Lcom/kamcord/android/KC_E;

    invoke-interface {v2}, Lcom/kamcord/android/KC_E;->getBufferPercentage()I

    move-result v2

    iget-object v3, p0, Lcom/kamcord/android/KC_w;->e:Landroid/widget/ProgressBar;

    mul-int/lit8 v2, v2, 0xa

    invoke-virtual {v3, v2}, Landroid/widget/ProgressBar;->setSecondaryProgress(I)V

    :cond_5
    iget-object v2, p0, Lcom/kamcord/android/KC_w;->g:Landroid/widget/TextView;

    if-eqz v2, :cond_6

    iget-object v2, p0, Lcom/kamcord/android/KC_w;->g:Landroid/widget/TextView;

    invoke-direct {p0, v1}, Lcom/kamcord/android/KC_w;->b(I)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v2, v1}, Landroid/widget/TextView;->setText(Ljava/lang/CharSequence;)V

    :cond_6
    iget-object v1, p0, Lcom/kamcord/android/KC_w;->h:Landroid/widget/TextView;

    if-eqz v1, :cond_1

    iget-object v1, p0, Lcom/kamcord/android/KC_w;->h:Landroid/widget/TextView;

    invoke-direct {p0, v0}, Lcom/kamcord/android/KC_w;->b(I)Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v1, v2}, Landroid/widget/TextView;->setText(Ljava/lang/CharSequence;)V

    goto :goto_0
.end method

.method public final f()V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->A:Landroid/os/Handler;

    const/4 v1, 0x2

    invoke-virtual {v0, v1}, Landroid/os/Handler;->removeMessages(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->A:Landroid/os/Handler;

    const/4 v1, 0x1

    invoke-virtual {v0, v1}, Landroid/os/Handler;->removeMessages(I)V

    return-void
.end method

.method public final g()V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->d:Landroid/view/View;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->s:Landroid/widget/LinearLayout;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->a:Lcom/kamcord/android/KC_E;

    if-nez v0, :cond_1

    :cond_0
    :goto_0
    return-void

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_w;->a:Lcom/kamcord/android/KC_E;

    invoke-interface {v0}, Lcom/kamcord/android/KC_E;->isPlaying()Z

    move-result v0

    if-eqz v0, :cond_2

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->r:Landroid/widget/ImageView;

    const-string v1, "drawable"

    const-string v2, "kamcord_player_pause_icon"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/ImageView;->setImageResource(I)V

    goto :goto_0

    :cond_2
    iget-object v0, p0, Lcom/kamcord/android/KC_w;->a:Lcom/kamcord/android/KC_E;

    invoke-interface {v0}, Lcom/kamcord/android/KC_E;->a()Z

    move-result v0

    if-eqz v0, :cond_3

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->r:Landroid/widget/ImageView;

    const-string v1, "drawable"

    const-string v2, "kamcord_player_replay_icon"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/ImageView;->setImageResource(I)V

    goto :goto_0

    :cond_3
    iget-object v0, p0, Lcom/kamcord/android/KC_w;->r:Landroid/widget/ImageView;

    const-string v1, "drawable"

    const-string v2, "kamcord_player_play_icon"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/ImageView;->setImageResource(I)V

    goto :goto_0
.end method

.method public final h()V
    .locals 4

    const/4 v3, 0x0

    iput-boolean v3, p0, Lcom/kamcord/android/KC_w;->l:Z

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->d:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "time"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/TextView;

    const-string v1, "kamcordMediaControlsSpacing"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord;->a(Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v3, v3, v1, v3}, Landroid/widget/TextView;->setPadding(IIII)V

    return-void
.end method

.method public onFinishInflate()V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->d:Landroid/view/View;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->d:Landroid/view/View;

    invoke-direct {p0, v0}, Lcom/kamcord/android/KC_w;->a(Landroid/view/View;)V

    :cond_0
    return-void
.end method

.method public onInitializeAccessibilityEvent(Landroid/view/accessibility/AccessibilityEvent;)V
    .locals 1

    invoke-super {p0, p1}, Landroid/widget/FrameLayout;->onInitializeAccessibilityEvent(Landroid/view/accessibility/AccessibilityEvent;)V

    const-class v0, Lcom/kamcord/android/KC_w;

    invoke-virtual {v0}, Ljava/lang/Class;->getName()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {p1, v0}, Landroid/view/accessibility/AccessibilityEvent;->setClassName(Ljava/lang/CharSequence;)V

    return-void
.end method

.method public onInitializeAccessibilityNodeInfo(Landroid/view/accessibility/AccessibilityNodeInfo;)V
    .locals 1

    invoke-super {p0, p1}, Landroid/widget/FrameLayout;->onInitializeAccessibilityNodeInfo(Landroid/view/accessibility/AccessibilityNodeInfo;)V

    const-class v0, Lcom/kamcord/android/KC_w;

    invoke-virtual {v0}, Ljava/lang/Class;->getName()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {p1, v0}, Landroid/view/accessibility/AccessibilityNodeInfo;->setClassName(Ljava/lang/CharSequence;)V

    return-void
.end method

.method public setEnabled(Z)V
    .locals 4

    const/4 v1, 0x1

    const/4 v2, 0x0

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->s:Landroid/widget/LinearLayout;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->s:Landroid/widget/LinearLayout;

    invoke-virtual {v0, p1}, Landroid/widget/LinearLayout;->setEnabled(Z)V

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_w;->w:Landroid/widget/LinearLayout;

    if-eqz v0, :cond_1

    iget-object v3, p0, Lcom/kamcord/android/KC_w;->w:Landroid/widget/LinearLayout;

    if-eqz p1, :cond_4

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->m:Landroid/view/View$OnClickListener;

    if-eqz v0, :cond_4

    move v0, v1

    :goto_0
    invoke-virtual {v3, v0}, Landroid/widget/LinearLayout;->setEnabled(Z)V

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_w;->u:Landroid/widget/LinearLayout;

    if-eqz v0, :cond_2

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->u:Landroid/widget/LinearLayout;

    if-eqz p1, :cond_5

    iget-object v3, p0, Lcom/kamcord/android/KC_w;->n:Landroid/view/View$OnClickListener;

    if-eqz v3, :cond_5

    :goto_1
    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->setEnabled(Z)V

    :cond_2
    iget-object v0, p0, Lcom/kamcord/android/KC_w;->e:Landroid/widget/ProgressBar;

    if-eqz v0, :cond_3

    iget-object v0, p0, Lcom/kamcord/android/KC_w;->e:Landroid/widget/ProgressBar;

    invoke-virtual {v0, p1}, Landroid/widget/ProgressBar;->setEnabled(Z)V

    :cond_3
    invoke-super {p0, p1}, Landroid/widget/FrameLayout;->setEnabled(Z)V

    return-void

    :cond_4
    move v0, v2

    goto :goto_0

    :cond_5
    move v1, v2

    goto :goto_1
.end method
