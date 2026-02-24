.class public abstract Lb/a/a/a/a/KC_b;
.super Ljava/lang/Object;
.source "SourceFile"


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lb/a/a/a/a/KC_b$KC_a;
    }
.end annotation


# instance fields
.field protected final a:I

.field private final b:I

.field private final c:I

.field private final d:I


# direct methods
.method protected constructor <init>(IIII)V
    .locals 2

    .prologue
    const/4 v0, 0x0

    .line 185
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 156
    const/4 v1, 0x3

    iput v1, p0, Lb/a/a/a/a/KC_b;->b:I

    .line 187
    const/4 v1, 0x4

    iput v1, p0, Lb/a/a/a/a/KC_b;->c:I

    .line 188
    if-lez p3, :cond_1

    if-lez p4, :cond_1

    const/4 v1, 0x1

    .line 189
    :goto_0
    if-eqz v1, :cond_0

    div-int/lit8 v0, p3, 0x4

    shl-int/lit8 v0, v0, 0x2

    :cond_0
    iput v0, p0, Lb/a/a/a/a/KC_b;->a:I

    .line 190
    iput p4, p0, Lb/a/a/a/a/KC_b;->d:I

    .line 191
    return-void

    :cond_1
    move v1, v0

    .line 188
    goto :goto_0
.end method


# virtual methods
.method abstract a([BIILb/a/a/a/a/KC_b$KC_a;)V
.end method

.method protected abstract a(B)Z
.end method

.method protected final a(ILb/a/a/a/a/KC_b$KC_a;)[B
    .locals 4

    .prologue
    const/4 v3, 0x0

    .line 246
    iget-object v0, p2, Lb/a/a/a/a/KC_b$KC_a;->b:[B

    if-eqz v0, :cond_0

    iget-object v0, p2, Lb/a/a/a/a/KC_b$KC_a;->b:[B

    array-length v0, v0

    iget v1, p2, Lb/a/a/a/a/KC_b$KC_a;->c:I

    add-int/2addr v1, p1

    if-ge v0, v1, :cond_2

    .line 247
    :cond_0
    iget-object v0, p2, Lb/a/a/a/a/KC_b$KC_a;->b:[B

    if-nez v0, :cond_1

    const/16 v0, 0x2000

    new-array v0, v0, [B

    iput-object v0, p2, Lb/a/a/a/a/KC_b$KC_a;->b:[B

    iput v3, p2, Lb/a/a/a/a/KC_b$KC_a;->c:I

    iput v3, p2, Lb/a/a/a/a/KC_b$KC_a;->d:I

    :goto_0
    iget-object v0, p2, Lb/a/a/a/a/KC_b$KC_a;->b:[B

    .line 249
    :goto_1
    return-object v0

    .line 247
    :cond_1
    iget-object v0, p2, Lb/a/a/a/a/KC_b$KC_a;->b:[B

    array-length v0, v0

    shl-int/lit8 v0, v0, 0x1

    new-array v0, v0, [B

    iget-object v1, p2, Lb/a/a/a/a/KC_b$KC_a;->b:[B

    iget-object v2, p2, Lb/a/a/a/a/KC_b$KC_a;->b:[B

    array-length v2, v2

    invoke-static {v1, v3, v0, v3, v2}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    iput-object v0, p2, Lb/a/a/a/a/KC_b$KC_a;->b:[B

    goto :goto_0

    .line 249
    :cond_2
    iget-object v0, p2, Lb/a/a/a/a/KC_b$KC_a;->b:[B

    goto :goto_1
.end method

.method protected final b([B)Z
    .locals 5

    .prologue
    const/4 v0, 0x0

    .line 473
    if-nez p1, :cond_1

    .line 481
    :cond_0
    :goto_0
    return v0

    .line 476
    :cond_1
    array-length v2, p1

    move v1, v0

    :goto_1
    if-ge v1, v2, :cond_0

    aget-byte v3, p1, v1

    .line 477
    const/16 v4, 0x3d

    if-eq v4, v3, :cond_2

    invoke-virtual {p0, v3}, Lb/a/a/a/a/KC_b;->a(B)Z

    move-result v3

    if-eqz v3, :cond_3

    .line 478
    :cond_2
    const/4 v0, 0x1

    goto :goto_0

    .line 476
    :cond_3
    add-int/lit8 v1, v1, 0x1

    goto :goto_1
.end method

.method public final c([B)J
    .locals 6

    .prologue
    .line 495
    array-length v0, p1

    iget v1, p0, Lb/a/a/a/a/KC_b;->b:I

    add-int/2addr v0, v1

    add-int/lit8 v0, v0, -0x1

    iget v1, p0, Lb/a/a/a/a/KC_b;->b:I

    div-int/2addr v0, v1

    int-to-long v0, v0

    iget v2, p0, Lb/a/a/a/a/KC_b;->c:I

    int-to-long v2, v2

    mul-long/2addr v0, v2

    .line 496
    iget v2, p0, Lb/a/a/a/a/KC_b;->a:I

    if-lez v2, :cond_0

    .line 498
    iget v2, p0, Lb/a/a/a/a/KC_b;->a:I

    int-to-long v2, v2

    add-long/2addr v2, v0

    const-wide/16 v4, 0x1

    sub-long/2addr v2, v4

    iget v4, p0, Lb/a/a/a/a/KC_b;->a:I

    int-to-long v4, v4

    div-long/2addr v2, v4

    iget v4, p0, Lb/a/a/a/a/KC_b;->d:I

    int-to-long v4, v4

    mul-long/2addr v2, v4

    add-long/2addr v0, v2

    .line 500
    :cond_0
    return-wide v0
.end method
