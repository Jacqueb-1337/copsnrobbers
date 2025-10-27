.class public final Lcom/kamcord/android/a/KC_b$KC_a;
.super Ljava/lang/Object;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/a/KC_b;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x9
    name = "KC_a"
.end annotation


# instance fields
.field private A:D

.field private a:Ljava/lang/String;

.field private b:Lcom/kamcord/android/core/KC_f;

.field private c:Ljava/lang/String;

.field private d:Ljava/lang/String;

.field private e:Ljava/lang/String;

.field private f:I

.field private g:Z

.field private h:Z

.field private i:Z

.field private j:Z

.field private k:Z

.field private l:I

.field private m:I

.field private n:I

.field private o:I

.field private p:I

.field private q:I

.field private r:I

.field private s:I

.field private t:I

.field private u:I

.field private v:I

.field private w:I

.field private x:I

.field private y:I

.field private z:I


# direct methods
.method public constructor <init>()V
    .locals 3

    const/4 v2, 0x1

    const/4 v1, 0x0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    const/4 v0, 0x0

    iput-object v0, p0, Lcom/kamcord/android/a/KC_b$KC_a;->c:Ljava/lang/String;

    const/4 v0, -0x1

    iput v0, p0, Lcom/kamcord/android/a/KC_b$KC_a;->f:I

    iput-boolean v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->g:Z

    iput-boolean v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->h:Z

    iput-boolean v2, p0, Lcom/kamcord/android/a/KC_b$KC_a;->i:Z

    iput-boolean v2, p0, Lcom/kamcord/android/a/KC_b$KC_a;->j:Z

    iput-boolean v2, p0, Lcom/kamcord/android/a/KC_b$KC_a;->k:Z

    iput v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->l:I

    iput v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->m:I

    iput v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->n:I

    iput v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->o:I

    iput v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->p:I

    iput v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->q:I

    iput v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->r:I

    iput v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->s:I

    iput v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->t:I

    iput v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->u:I

    iput v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->v:I

    iput v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->w:I

    iput v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->x:I

    const/4 v0, 0x2

    iput v0, p0, Lcom/kamcord/android/a/KC_b$KC_a;->y:I

    const/16 v0, 0x12

    iput v0, p0, Lcom/kamcord/android/a/KC_b$KC_a;->z:I

    const-wide v0, 0x3f9b4e81b4f6c44eL    # 0.02666666667

    iput-wide v0, p0, Lcom/kamcord/android/a/KC_b$KC_a;->A:D

    return-void
.end method


# virtual methods
.method public final a(I)Lcom/kamcord/android/a/KC_b$KC_a;
    .locals 0

    iput p1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->f:I

    return-object p0
.end method

.method public final a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->b:Lcom/kamcord/android/core/KC_f;

    return-object p0
.end method

.method public final a(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->c:Ljava/lang/String;

    return-object p0
.end method

.method public final a(Z)Lcom/kamcord/android/a/KC_b$KC_a;
    .locals 1

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/a/KC_b$KC_a;->g:Z

    return-object p0
.end method

.method public final a()Lcom/kamcord/android/a/KC_b;
    .locals 5

    iget v0, p0, Lcom/kamcord/android/a/KC_b$KC_a;->l:I

    if-lez v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/a/KC_b$KC_a;->b:Lcom/kamcord/android/core/KC_f;

    if-nez v0, :cond_1

    const-string v0, "You must include dimensions when using a codec padding rule! Ignoring padding rule."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    :cond_0
    :goto_0
    new-instance v0, Lcom/kamcord/android/a/KC_b;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b;-><init>()V

    iget-object v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->a:Ljava/lang/String;

    iget-object v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->b:Lcom/kamcord/android/core/KC_f;

    iput-object v1, v0, Lcom/kamcord/android/a/KC_b;->a:Lcom/kamcord/android/core/KC_f;

    iget-object v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->c:Ljava/lang/String;

    iput-object v1, v0, Lcom/kamcord/android/a/KC_b;->b:Ljava/lang/String;

    iget-object v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->d:Ljava/lang/String;

    iput-object v1, v0, Lcom/kamcord/android/a/KC_b;->c:Ljava/lang/String;

    iget-object v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->e:Ljava/lang/String;

    iput-object v1, v0, Lcom/kamcord/android/a/KC_b;->d:Ljava/lang/String;

    iget v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->f:I

    iput v1, v0, Lcom/kamcord/android/a/KC_b;->e:I

    iget-boolean v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->g:Z

    iput-boolean v1, v0, Lcom/kamcord/android/a/KC_b;->f:Z

    iget-boolean v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->h:Z

    iput-boolean v1, v0, Lcom/kamcord/android/a/KC_b;->g:Z

    iget v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->m:I

    iput v1, v0, Lcom/kamcord/android/a/KC_b;->h:I

    iget v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->n:I

    iput v1, v0, Lcom/kamcord/android/a/KC_b;->i:I

    iget v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->o:I

    iput v1, v0, Lcom/kamcord/android/a/KC_b;->j:I

    iget v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->p:I

    iput v1, v0, Lcom/kamcord/android/a/KC_b;->k:I

    iget v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->q:I

    iput v1, v0, Lcom/kamcord/android/a/KC_b;->l:I

    iget v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->r:I

    iput v1, v0, Lcom/kamcord/android/a/KC_b;->m:I

    iget v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->s:I

    iput v1, v0, Lcom/kamcord/android/a/KC_b;->n:I

    iget v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->t:I

    iput v1, v0, Lcom/kamcord/android/a/KC_b;->o:I

    iget v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->u:I

    iput v1, v0, Lcom/kamcord/android/a/KC_b;->p:I

    iget v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->v:I

    iput v1, v0, Lcom/kamcord/android/a/KC_b;->q:I

    iget v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->w:I

    iput v1, v0, Lcom/kamcord/android/a/KC_b;->r:I

    iget v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->x:I

    iput v1, v0, Lcom/kamcord/android/a/KC_b;->s:I

    iget-boolean v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->i:Z

    iput-boolean v1, v0, Lcom/kamcord/android/a/KC_b;->v:Z

    iget-boolean v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->j:Z

    iput-boolean v1, v0, Lcom/kamcord/android/a/KC_b;->w:Z

    iget-boolean v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->k:Z

    iput-boolean v1, v0, Lcom/kamcord/android/a/KC_b;->x:Z

    iget v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->y:I

    iput v1, v0, Lcom/kamcord/android/a/KC_b;->t:I

    iget v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->z:I

    iput v1, v0, Lcom/kamcord/android/a/KC_b;->u:I

    iget-wide v2, p0, Lcom/kamcord/android/a/KC_b$KC_a;->A:D

    iput-wide v2, v0, Lcom/kamcord/android/a/KC_b;->y:D

    return-object v0

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/a/KC_b$KC_a;->b:Lcom/kamcord/android/core/KC_f;

    iget v0, v0, Lcom/kamcord/android/core/KC_f;->b:I

    rem-int/lit8 v0, v0, 0x2

    iget-object v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->b:Lcom/kamcord/android/core/KC_f;

    iget v1, v1, Lcom/kamcord/android/core/KC_f;->a:I

    rem-int/lit8 v1, v1, 0x2

    add-int/2addr v0, v1

    if-lez v0, :cond_2

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Video dimensions should be even! Current dimensions: "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->b:Lcom/kamcord/android/core/KC_f;

    iget v1, v1, Lcom/kamcord/android/core/KC_f;->a:I

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, "x"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->b:Lcom/kamcord/android/core/KC_f;

    iget v1, v1, Lcom/kamcord/android/core/KC_f;->b:I

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    :cond_2
    iget-object v0, p0, Lcom/kamcord/android/a/KC_b$KC_a;->b:Lcom/kamcord/android/core/KC_f;

    iget v0, v0, Lcom/kamcord/android/core/KC_f;->b:I

    iget-object v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->b:Lcom/kamcord/android/core/KC_f;

    iget v1, v1, Lcom/kamcord/android/core/KC_f;->a:I

    if-ge v0, v1, :cond_5

    iget-object v0, p0, Lcom/kamcord/android/a/KC_b$KC_a;->b:Lcom/kamcord/android/core/KC_f;

    iget v0, v0, Lcom/kamcord/android/core/KC_f;->b:I

    :goto_1
    iget-object v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->b:Lcom/kamcord/android/core/KC_f;

    iget v1, v1, Lcom/kamcord/android/core/KC_f;->b:I

    iget-object v2, p0, Lcom/kamcord/android/a/KC_b$KC_a;->b:Lcom/kamcord/android/core/KC_f;

    iget v2, v2, Lcom/kamcord/android/core/KC_f;->a:I

    if-le v1, v2, :cond_6

    iget-object v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->b:Lcom/kamcord/android/core/KC_f;

    iget v1, v1, Lcom/kamcord/android/core/KC_f;->b:I

    :goto_2
    iget v2, p0, Lcom/kamcord/android/a/KC_b$KC_a;->l:I

    and-int/lit8 v2, v2, 0x4

    if-lez v2, :cond_a

    add-int/lit8 v2, v0, 0xf

    and-int/lit8 v2, v2, -0x10

    :goto_3
    iput v2, p0, Lcom/kamcord/android/a/KC_b$KC_a;->q:I

    iput v2, p0, Lcom/kamcord/android/a/KC_b$KC_a;->r:I

    div-int/lit8 v3, v2, 0x2

    iput v3, p0, Lcom/kamcord/android/a/KC_b$KC_a;->s:I

    div-int/lit8 v2, v2, 0x2

    iput v2, p0, Lcom/kamcord/android/a/KC_b$KC_a;->t:I

    iget v2, p0, Lcom/kamcord/android/a/KC_b$KC_a;->l:I

    and-int/lit8 v2, v2, 0x1

    if-lez v2, :cond_9

    add-int/lit8 v2, v1, 0xf

    and-int/lit8 v2, v2, -0x10

    :goto_4
    iget v3, p0, Lcom/kamcord/android/a/KC_b$KC_a;->q:I

    mul-int/2addr v3, v2

    iget v4, p0, Lcom/kamcord/android/a/KC_b$KC_a;->l:I

    and-int/lit8 v4, v4, 0x2

    if-lez v4, :cond_3

    add-int/lit16 v3, v3, 0x7ff

    and-int/lit16 v3, v3, -0x800

    :cond_3
    mul-int v4, v0, v1

    sub-int/2addr v3, v4

    iput v3, p0, Lcom/kamcord/android/a/KC_b$KC_a;->m:I

    iget v3, p0, Lcom/kamcord/android/a/KC_b$KC_a;->s:I

    mul-int/2addr v2, v3

    div-int/lit8 v2, v2, 0x2

    div-int/lit8 v3, v0, 0x2

    mul-int/2addr v3, v1

    div-int/lit8 v3, v3, 0x2

    sub-int/2addr v2, v3

    iput v2, p0, Lcom/kamcord/android/a/KC_b$KC_a;->o:I

    iget v2, p0, Lcom/kamcord/android/a/KC_b$KC_a;->l:I

    and-int/lit8 v2, v2, 0x4

    if-lez v2, :cond_8

    add-int/lit8 v2, v1, 0xf

    and-int/lit8 v2, v2, -0x10

    :goto_5
    iput v2, p0, Lcom/kamcord/android/a/KC_b$KC_a;->u:I

    iput v2, p0, Lcom/kamcord/android/a/KC_b$KC_a;->v:I

    div-int/lit8 v3, v2, 0x2

    iput v3, p0, Lcom/kamcord/android/a/KC_b$KC_a;->w:I

    div-int/lit8 v2, v2, 0x2

    iput v2, p0, Lcom/kamcord/android/a/KC_b$KC_a;->x:I

    iget v2, p0, Lcom/kamcord/android/a/KC_b$KC_a;->l:I

    and-int/lit8 v2, v2, 0x1

    if-lez v2, :cond_7

    add-int/lit8 v2, v0, 0xf

    and-int/lit8 v2, v2, -0x10

    :goto_6
    iget v3, p0, Lcom/kamcord/android/a/KC_b$KC_a;->u:I

    mul-int/2addr v3, v2

    iget v4, p0, Lcom/kamcord/android/a/KC_b$KC_a;->l:I

    and-int/lit8 v4, v4, 0x2

    if-lez v4, :cond_4

    add-int/lit16 v3, v3, 0x7ff

    and-int/lit16 v3, v3, -0x800

    :cond_4
    mul-int v4, v0, v1

    sub-int/2addr v3, v4

    iput v3, p0, Lcom/kamcord/android/a/KC_b$KC_a;->n:I

    iget v3, p0, Lcom/kamcord/android/a/KC_b$KC_a;->w:I

    mul-int/2addr v2, v3

    div-int/lit8 v2, v2, 0x2

    div-int/lit8 v1, v1, 0x2

    mul-int/2addr v0, v1

    div-int/lit8 v0, v0, 0x2

    sub-int v0, v2, v0

    iput v0, p0, Lcom/kamcord/android/a/KC_b$KC_a;->p:I

    goto/16 :goto_0

    :cond_5
    iget-object v0, p0, Lcom/kamcord/android/a/KC_b$KC_a;->b:Lcom/kamcord/android/core/KC_f;

    iget v0, v0, Lcom/kamcord/android/core/KC_f;->a:I

    goto/16 :goto_1

    :cond_6
    iget-object v1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->b:Lcom/kamcord/android/core/KC_f;

    iget v1, v1, Lcom/kamcord/android/core/KC_f;->a:I

    goto/16 :goto_2

    :cond_7
    move v2, v0

    goto :goto_6

    :cond_8
    move v2, v1

    goto :goto_5

    :cond_9
    move v2, v1

    goto :goto_4

    :cond_a
    move v2, v0

    goto/16 :goto_3
.end method

.method public final b(I)Lcom/kamcord/android/a/KC_b$KC_a;
    .locals 0

    iput p1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->l:I

    return-object p0
.end method

.method public final b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->d:Ljava/lang/String;

    return-object p0
.end method

.method public final b(Z)Lcom/kamcord/android/a/KC_b$KC_a;
    .locals 1

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/a/KC_b$KC_a;->h:Z

    return-object p0
.end method

.method public final c(I)Lcom/kamcord/android/a/KC_b$KC_a;
    .locals 1

    const v0, 0x7fffffff

    iput v0, p0, Lcom/kamcord/android/a/KC_b$KC_a;->z:I

    return-object p0
.end method

.method public final c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->e:Ljava/lang/String;

    return-object p0
.end method

.method public final c(Z)Lcom/kamcord/android/a/KC_b$KC_a;
    .locals 1

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/a/KC_b$KC_a;->i:Z

    return-object p0
.end method

.method public final d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/a/KC_b$KC_a;->a:Ljava/lang/String;

    return-object p0
.end method

.method public final d(Z)Lcom/kamcord/android/a/KC_b$KC_a;
    .locals 1

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/a/KC_b$KC_a;->j:Z

    return-object p0
.end method

.method public final e(Z)Lcom/kamcord/android/a/KC_b$KC_a;
    .locals 1

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/a/KC_b$KC_a;->k:Z

    return-object p0
.end method
