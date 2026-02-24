.class public final La/a/a/d/KC_f;
.super Ljava/lang/Object;
.source "SourceFile"

# interfaces
.implements Ljava/lang/Cloneable;


# annotations
.annotation system Ldalvik/annotation/Signature;
    value = {
        "<E:",
        "Ljava/lang/Object;",
        ">",
        "Ljava/lang/Object;",
        "Ljava/lang/Cloneable;"
    }
.end annotation


# static fields
.field private static final a:Ljava/lang/Object;


# instance fields
.field private b:Z

.field private c:[I

.field private d:[Ljava/lang/Object;

.field private e:I


# direct methods
.method static constructor <clinit>()V
    .locals 1

    .prologue
    .line 24
    new-instance v0, Ljava/lang/Object;

    invoke-direct {v0}, Ljava/lang/Object;-><init>()V

    sput-object v0, La/a/a/d/KC_f;->a:Ljava/lang/Object;

    return-void
.end method

.method public constructor <init>()V
    .locals 1

    .prologue
    .line 35
    const/16 v0, 0xa

    invoke-direct {p0, v0}, La/a/a/d/KC_f;-><init>(I)V

    .line 36
    return-void
.end method

.method private constructor <init>(I)V
    .locals 3

    .prologue
    const/4 v2, 0x0

    .line 45
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 25
    iput-boolean v2, p0, La/a/a/d/KC_f;->b:Z

    .line 46
    const/16 v0, 0xa

    invoke-static {v0}, La/a/a/d/KC_b;->a(I)I

    move-result v0

    .line 51
    new-array v1, v0, [I

    iput-object v1, p0, La/a/a/d/KC_f;->c:[I

    .line 52
    new-array v0, v0, [Ljava/lang/Object;

    iput-object v0, p0, La/a/a/d/KC_f;->d:[Ljava/lang/Object;

    .line 54
    iput v2, p0, La/a/a/d/KC_f;->e:I

    .line 55
    return-void
.end method

.method private c()La/a/a/d/KC_f;
    .locals 2
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "()",
            "La/a/a/d/KC_f",
            "<TE;>;"
        }
    .end annotation

    .prologue
    .line 60
    const/4 v1, 0x0

    .line 62
    :try_start_0
    invoke-super {p0}, Ljava/lang/Object;->clone()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/d/KC_f;
    :try_end_0
    .catch Ljava/lang/CloneNotSupportedException; {:try_start_0 .. :try_end_0} :catch_0

    .line 63
    :try_start_1
    iget-object v1, p0, La/a/a/d/KC_f;->c:[I

    invoke-virtual {v1}, [I->clone()Ljava/lang/Object;

    move-result-object v1

    check-cast v1, [I

    iput-object v1, v0, La/a/a/d/KC_f;->c:[I

    .line 64
    iget-object v1, p0, La/a/a/d/KC_f;->d:[Ljava/lang/Object;

    invoke-virtual {v1}, [Ljava/lang/Object;->clone()Ljava/lang/Object;

    move-result-object v1

    check-cast v1, [Ljava/lang/Object;

    iput-object v1, v0, La/a/a/d/KC_f;->d:[Ljava/lang/Object;
    :try_end_1
    .catch Ljava/lang/CloneNotSupportedException; {:try_start_1 .. :try_end_1} :catch_1

    .line 68
    :goto_0
    return-object v0

    :catch_0
    move-exception v0

    move-object v0, v1

    goto :goto_0

    :catch_1
    move-exception v1

    goto :goto_0
.end method


# virtual methods
.method public final a()I
    .locals 1

    .prologue
    .line 223
    iget v0, p0, La/a/a/d/KC_f;->e:I

    return v0
.end method

.method public final a(I)I
    .locals 1

    .prologue
    .line 236
    iget-object v0, p0, La/a/a/d/KC_f;->c:[I

    aget v0, v0, p1

    return v0
.end method

.method public final b(I)Ljava/lang/Object;
    .locals 1
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(I)TE;"
        }
    .end annotation

    .prologue
    .line 250
    iget-object v0, p0, La/a/a/d/KC_f;->d:[Ljava/lang/Object;

    aget-object v0, v0, p1

    return-object v0
.end method

.method public final b()V
    .locals 5

    .prologue
    const/4 v1, 0x0

    .line 309
    iget v2, p0, La/a/a/d/KC_f;->e:I

    .line 310
    iget-object v3, p0, La/a/a/d/KC_f;->d:[Ljava/lang/Object;

    move v0, v1

    .line 312
    :goto_0
    if-ge v0, v2, :cond_0

    .line 313
    const/4 v4, 0x0

    aput-object v4, v3, v0

    .line 312
    add-int/lit8 v0, v0, 0x1

    goto :goto_0

    .line 316
    :cond_0
    iput v1, p0, La/a/a/d/KC_f;->e:I

    .line 317
    iput-boolean v1, p0, La/a/a/d/KC_f;->b:Z

    .line 318
    return-void
.end method

.method public final synthetic clone()Ljava/lang/Object;
    .locals 1
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/lang/CloneNotSupportedException;
        }
    .end annotation

    .prologue
    .line 23
    invoke-direct {p0}, La/a/a/d/KC_f;->c()La/a/a/d/KC_f;

    move-result-object v0

    return-object v0
.end method

.method public final toString()Ljava/lang/String;
    .locals 3

    .prologue
    .line 363
    iget v0, p0, La/a/a/d/KC_f;->e:I

    if-gtz v0, :cond_0

    .line 364
    const-string v0, "{}"

    .line 384
    :goto_0
    return-object v0

    .line 367
    :cond_0
    new-instance v1, Ljava/lang/StringBuilder;

    iget v0, p0, La/a/a/d/KC_f;->e:I

    mul-int/lit8 v0, v0, 0x1c

    invoke-direct {v1, v0}, Ljava/lang/StringBuilder;-><init>(I)V

    .line 368
    const/16 v0, 0x7b

    invoke-virtual {v1, v0}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 369
    const/4 v0, 0x0

    :goto_1
    iget v2, p0, La/a/a/d/KC_f;->e:I

    if-ge v0, v2, :cond_3

    .line 370
    if-lez v0, :cond_1

    .line 371
    const-string v2, ", "

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 373
    :cond_1
    iget-object v2, p0, La/a/a/d/KC_f;->c:[I

    aget v2, v2, v0

    .line 374
    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    .line 375
    const/16 v2, 0x3d

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 376
    iget-object v2, p0, La/a/a/d/KC_f;->d:[Ljava/lang/Object;

    aget-object v2, v2, v0

    .line 377
    if-eq v2, p0, :cond_2

    .line 378
    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    .line 369
    :goto_2
    add-int/lit8 v0, v0, 0x1

    goto :goto_1

    .line 380
    :cond_2
    const-string v2, "(this Map)"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    goto :goto_2

    .line 383
    :cond_3
    const/16 v0, 0x7d

    invoke-virtual {v1, v0}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 384
    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    goto :goto_0
.end method
