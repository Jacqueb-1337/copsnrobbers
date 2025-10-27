.class public La/a/a/d/KC_e;
.super Ljava/lang/Object;
.source "SourceFile"


# annotations
.annotation system Ldalvik/annotation/Signature;
    value = {
        "<K:",
        "Ljava/lang/Object;",
        "V:",
        "Ljava/lang/Object;",
        ">",
        "Ljava/lang/Object;"
    }
.end annotation


# static fields
.field private static c:[Ljava/lang/Object;

.field private static d:I

.field private static e:[Ljava/lang/Object;

.field private static f:I


# instance fields
.field a:[Ljava/lang/Object;

.field b:I

.field private g:[I


# direct methods
.method public constructor <init>()V
    .locals 1

    .prologue
    .line 208
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 209
    sget-object v0, La/a/a/d/KC_b;->a:[I

    iput-object v0, p0, La/a/a/d/KC_e;->g:[I

    .line 210
    sget-object v0, La/a/a/d/KC_b;->b:[Ljava/lang/Object;

    iput-object v0, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    .line 211
    const/4 v0, 0x0

    iput v0, p0, La/a/a/d/KC_e;->b:I

    .line 212
    return-void
.end method

.method private static a([I[Ljava/lang/Object;I)V
    .locals 4

    .prologue
    const/16 v2, 0xa

    const/4 v3, 0x2

    .line 173
    array-length v0, p0

    const/16 v1, 0x8

    if-ne v0, v1, :cond_3

    .line 174
    const-class v1, La/a/a/d/KC_a;

    monitor-enter v1

    .line 175
    :try_start_0
    sget v0, La/a/a/d/KC_e;->f:I

    if-ge v0, v2, :cond_1

    .line 176
    const/4 v0, 0x0

    sget-object v2, La/a/a/d/KC_e;->e:[Ljava/lang/Object;

    aput-object v2, p1, v0

    .line 177
    const/4 v0, 0x1

    aput-object p0, p1, v0

    .line 178
    shl-int/lit8 v0, p2, 0x1

    add-int/lit8 v0, v0, -0x1

    :goto_0
    if-lt v0, v3, :cond_0

    .line 179
    const/4 v2, 0x0

    aput-object v2, p1, v0

    .line 178
    add-int/lit8 v0, v0, -0x1

    goto :goto_0

    .line 181
    :cond_0
    sput-object p1, La/a/a/d/KC_e;->e:[Ljava/lang/Object;

    .line 182
    sget v0, La/a/a/d/KC_e;->f:I

    add-int/lit8 v0, v0, 0x1

    sput v0, La/a/a/d/KC_e;->f:I

    .line 186
    :cond_1
    monitor-exit v1
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    .line 202
    :cond_2
    :goto_1
    return-void

    .line 186
    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0

    .line 187
    :cond_3
    array-length v0, p0

    const/4 v1, 0x4

    if-ne v0, v1, :cond_2

    .line 188
    const-class v1, La/a/a/d/KC_a;

    monitor-enter v1

    .line 189
    :try_start_1
    sget v0, La/a/a/d/KC_e;->d:I

    if-ge v0, v2, :cond_5

    .line 190
    const/4 v0, 0x0

    sget-object v2, La/a/a/d/KC_e;->c:[Ljava/lang/Object;

    aput-object v2, p1, v0

    .line 191
    const/4 v0, 0x1

    aput-object p0, p1, v0

    .line 192
    shl-int/lit8 v0, p2, 0x1

    add-int/lit8 v0, v0, -0x1

    :goto_2
    if-lt v0, v3, :cond_4

    .line 193
    const/4 v2, 0x0

    aput-object v2, p1, v0

    .line 192
    add-int/lit8 v0, v0, -0x1

    goto :goto_2

    .line 195
    :cond_4
    sput-object p1, La/a/a/d/KC_e;->c:[Ljava/lang/Object;

    .line 196
    sget v0, La/a/a/d/KC_e;->d:I

    add-int/lit8 v0, v0, 0x1

    sput v0, La/a/a/d/KC_e;->d:I

    .line 200
    :cond_5
    monitor-exit v1
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_1

    goto :goto_1

    :catchall_1
    move-exception v0

    monitor-exit v1

    throw v0
.end method

.method private d(I)V
    .locals 5

    .prologue
    .line 138
    const/16 v0, 0x8

    if-ne p1, v0, :cond_2

    .line 139
    const-class v1, La/a/a/d/KC_a;

    monitor-enter v1

    .line 140
    :try_start_0
    sget-object v0, La/a/a/d/KC_e;->e:[Ljava/lang/Object;

    if-eqz v0, :cond_0

    .line 141
    sget-object v2, La/a/a/d/KC_e;->e:[Ljava/lang/Object;

    .line 142
    iput-object v2, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    .line 143
    const/4 v0, 0x0

    aget-object v0, v2, v0

    check-cast v0, [Ljava/lang/Object;

    sput-object v0, La/a/a/d/KC_e;->e:[Ljava/lang/Object;

    .line 144
    const/4 v0, 0x1

    aget-object v0, v2, v0

    check-cast v0, [I

    iput-object v0, p0, La/a/a/d/KC_e;->g:[I

    .line 145
    const/4 v0, 0x0

    const/4 v3, 0x1

    const/4 v4, 0x0

    aput-object v4, v2, v3

    aput-object v4, v2, v0

    .line 146
    sget v0, La/a/a/d/KC_e;->f:I

    add-int/lit8 v0, v0, -0x1

    sput v0, La/a/a/d/KC_e;->f:I

    .line 149
    monitor-exit v1
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    .line 170
    :goto_0
    return-void

    .line 151
    :cond_0
    monitor-exit v1

    .line 168
    :cond_1
    :goto_1
    new-array v0, p1, [I

    iput-object v0, p0, La/a/a/d/KC_e;->g:[I

    .line 169
    shl-int/lit8 v0, p1, 0x1

    new-array v0, v0, [Ljava/lang/Object;

    iput-object v0, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    goto :goto_0

    .line 151
    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0

    .line 152
    :cond_2
    const/4 v0, 0x4

    if-ne p1, v0, :cond_1

    .line 153
    const-class v1, La/a/a/d/KC_a;

    monitor-enter v1

    .line 154
    :try_start_1
    sget-object v0, La/a/a/d/KC_e;->c:[Ljava/lang/Object;

    if-eqz v0, :cond_3

    .line 155
    sget-object v2, La/a/a/d/KC_e;->c:[Ljava/lang/Object;

    .line 156
    iput-object v2, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    .line 157
    const/4 v0, 0x0

    aget-object v0, v2, v0

    check-cast v0, [Ljava/lang/Object;

    sput-object v0, La/a/a/d/KC_e;->c:[Ljava/lang/Object;

    .line 158
    const/4 v0, 0x1

    aget-object v0, v2, v0

    check-cast v0, [I

    iput-object v0, p0, La/a/a/d/KC_e;->g:[I

    .line 159
    const/4 v0, 0x0

    const/4 v3, 0x1

    const/4 v4, 0x0

    aput-object v4, v2, v3

    aput-object v4, v2, v0

    .line 160
    sget v0, La/a/a/d/KC_e;->d:I

    add-int/lit8 v0, v0, -0x1

    sput v0, La/a/a/d/KC_e;->d:I

    .line 163
    monitor-exit v1
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_1

    goto :goto_0

    .line 165
    :catchall_1
    move-exception v0

    monitor-exit v1

    throw v0

    :cond_3
    monitor-exit v1

    goto :goto_1
.end method

.method private e(I)Ljava/lang/Object;
    .locals 2
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(I)TK;"
        }
    .end annotation

    .prologue
    .line 323
    iget-object v0, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    shl-int/lit8 v1, p1, 0x1

    aget-object v0, v0, v1

    return-object v0
.end method


# virtual methods
.method final a()I
    .locals 5

    .prologue
    .line 100
    iget v2, p0, La/a/a/d/KC_e;->b:I

    .line 103
    if-nez v2, :cond_1

    .line 104
    const/4 v0, -0x1

    .line 134
    :cond_0
    :goto_0
    return v0

    .line 107
    :cond_1
    iget-object v0, p0, La/a/a/d/KC_e;->g:[I

    const/4 v1, 0x0

    invoke-static {v0, v2, v1}, La/a/a/d/KC_b;->a([III)I

    move-result v0

    .line 110
    if-ltz v0, :cond_0

    .line 115
    iget-object v1, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    shl-int/lit8 v3, v0, 0x1

    aget-object v1, v1, v3

    if-eqz v1, :cond_0

    .line 121
    add-int/lit8 v1, v0, 0x1

    :goto_1
    if-ge v1, v2, :cond_3

    iget-object v3, p0, La/a/a/d/KC_e;->g:[I

    aget v3, v3, v1

    if-nez v3, :cond_3

    .line 122
    iget-object v3, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    shl-int/lit8 v4, v1, 0x1

    aget-object v3, v3, v4

    if-nez v3, :cond_2

    move v0, v1

    goto :goto_0

    .line 121
    :cond_2
    add-int/lit8 v1, v1, 0x1

    goto :goto_1

    .line 126
    :cond_3
    add-int/lit8 v0, v0, -0x1

    :goto_2
    if-ltz v0, :cond_4

    iget-object v2, p0, La/a/a/d/KC_e;->g:[I

    aget v2, v2, v0

    if-nez v2, :cond_4

    .line 127
    iget-object v2, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    shl-int/lit8 v3, v0, 0x1

    aget-object v2, v2, v3

    if-eqz v2, :cond_0

    .line 126
    add-int/lit8 v0, v0, -0x1

    goto :goto_2

    .line 134
    :cond_4
    xor-int/lit8 v0, v1, -0x1

    goto :goto_0
.end method

.method final a(Ljava/lang/Object;)I
    .locals 4

    .prologue
    const/4 v0, 0x1

    .line 277
    iget v1, p0, La/a/a/d/KC_e;->b:I

    shl-int/lit8 v1, v1, 0x1

    .line 278
    iget-object v2, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    .line 279
    if-nez p1, :cond_2

    .line 280
    :goto_0
    if-ge v0, v1, :cond_3

    .line 281
    aget-object v3, v2, v0

    if-nez v3, :cond_0

    .line 282
    shr-int/lit8 v0, v0, 0x1

    .line 292
    :goto_1
    return v0

    .line 280
    :cond_0
    add-int/lit8 v0, v0, 0x2

    goto :goto_0

    .line 286
    :cond_1
    add-int/lit8 v0, v0, 0x2

    :cond_2
    if-ge v0, v1, :cond_3

    .line 287
    aget-object v3, v2, v0

    invoke-virtual {p1, v3}, Ljava/lang/Object;->equals(Ljava/lang/Object;)Z

    move-result v3

    if-eqz v3, :cond_1

    .line 288
    shr-int/lit8 v0, v0, 0x1

    goto :goto_1

    .line 292
    :cond_3
    const/4 v0, -0x1

    goto :goto_1
.end method

.method final a(Ljava/lang/Object;I)I
    .locals 5

    .prologue
    .line 62
    iget v2, p0, La/a/a/d/KC_e;->b:I

    .line 65
    if-nez v2, :cond_1

    .line 66
    const/4 v0, -0x1

    .line 96
    :cond_0
    :goto_0
    return v0

    .line 69
    :cond_1
    iget-object v0, p0, La/a/a/d/KC_e;->g:[I

    invoke-static {v0, v2, p2}, La/a/a/d/KC_b;->a([III)I

    move-result v0

    .line 72
    if-ltz v0, :cond_0

    .line 77
    iget-object v1, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    shl-int/lit8 v3, v0, 0x1

    aget-object v1, v1, v3

    invoke-virtual {p1, v1}, Ljava/lang/Object;->equals(Ljava/lang/Object;)Z

    move-result v1

    if-nez v1, :cond_0

    .line 83
    add-int/lit8 v1, v0, 0x1

    :goto_1
    if-ge v1, v2, :cond_3

    iget-object v3, p0, La/a/a/d/KC_e;->g:[I

    aget v3, v3, v1

    if-ne v3, p2, :cond_3

    .line 84
    iget-object v3, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    shl-int/lit8 v4, v1, 0x1

    aget-object v3, v3, v4

    invoke-virtual {p1, v3}, Ljava/lang/Object;->equals(Ljava/lang/Object;)Z

    move-result v3

    if-eqz v3, :cond_2

    move v0, v1

    goto :goto_0

    .line 83
    :cond_2
    add-int/lit8 v1, v1, 0x1

    goto :goto_1

    .line 88
    :cond_3
    add-int/lit8 v0, v0, -0x1

    :goto_2
    if-ltz v0, :cond_4

    iget-object v2, p0, La/a/a/d/KC_e;->g:[I

    aget v2, v2, v0

    if-ne v2, p2, :cond_4

    .line 89
    iget-object v2, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    shl-int/lit8 v3, v0, 0x1

    aget-object v2, v2, v3

    invoke-virtual {p1, v2}, Ljava/lang/Object;->equals(Ljava/lang/Object;)Z

    move-result v2

    if-nez v2, :cond_0

    .line 88
    add-int/lit8 v0, v0, -0x1

    goto :goto_2

    .line 96
    :cond_4
    xor-int/lit8 v0, v1, -0x1

    goto :goto_0
.end method

.method public final a(I)V
    .locals 5

    .prologue
    const/4 v4, 0x0

    .line 254
    iget-object v0, p0, La/a/a/d/KC_e;->g:[I

    array-length v0, v0

    if-ge v0, p1, :cond_1

    .line 255
    iget-object v0, p0, La/a/a/d/KC_e;->g:[I

    .line 256
    iget-object v1, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    .line 257
    invoke-direct {p0, p1}, La/a/a/d/KC_e;->d(I)V

    .line 258
    iget v2, p0, La/a/a/d/KC_e;->b:I

    if-lez v2, :cond_0

    .line 259
    iget-object v2, p0, La/a/a/d/KC_e;->g:[I

    iget v3, p0, La/a/a/d/KC_e;->b:I

    invoke-static {v0, v4, v2, v4, v3}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    .line 260
    iget-object v2, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    iget v3, p0, La/a/a/d/KC_e;->b:I

    shl-int/lit8 v3, v3, 0x1

    invoke-static {v1, v4, v2, v4, v3}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    .line 262
    :cond_0
    iget v2, p0, La/a/a/d/KC_e;->b:I

    invoke-static {v0, v1, v2}, La/a/a/d/KC_e;->a([I[Ljava/lang/Object;I)V

    .line 264
    :cond_1
    return-void
.end method

.method public final b(I)Ljava/lang/Object;
    .locals 2
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(I)TV;"
        }
    .end annotation

    .prologue
    .line 332
    iget-object v0, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    shl-int/lit8 v1, p1, 0x1

    add-int/lit8 v1, v1, 0x1

    aget-object v0, v0, v1

    return-object v0
.end method

.method public final c(I)Ljava/lang/Object;
    .locals 7
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(I)TV;"
        }
    .end annotation

    .prologue
    const/4 v6, 0x0

    const/16 v0, 0x8

    const/4 v5, 0x0

    .line 455
    iget-object v1, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    shl-int/lit8 v2, p1, 0x1

    add-int/lit8 v2, v2, 0x1

    aget-object v1, v1, v2

    .line 456
    iget v2, p0, La/a/a/d/KC_e;->b:I

    const/4 v3, 0x1

    if-gt v2, v3, :cond_1

    .line 459
    iget-object v0, p0, La/a/a/d/KC_e;->g:[I

    iget-object v2, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    iget v3, p0, La/a/a/d/KC_e;->b:I

    invoke-static {v0, v2, v3}, La/a/a/d/KC_e;->a([I[Ljava/lang/Object;I)V

    .line 460
    sget-object v0, La/a/a/d/KC_b;->a:[I

    iput-object v0, p0, La/a/a/d/KC_e;->g:[I

    .line 461
    sget-object v0, La/a/a/d/KC_b;->b:[Ljava/lang/Object;

    iput-object v0, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    .line 462
    iput v5, p0, La/a/a/d/KC_e;->b:I

    .line 502
    :cond_0
    :goto_0
    return-object v1

    .line 464
    :cond_1
    iget-object v2, p0, La/a/a/d/KC_e;->g:[I

    array-length v2, v2

    if-le v2, v0, :cond_4

    iget v2, p0, La/a/a/d/KC_e;->b:I

    iget-object v3, p0, La/a/a/d/KC_e;->g:[I

    array-length v3, v3

    div-int/lit8 v3, v3, 0x3

    if-ge v2, v3, :cond_4

    .line 468
    iget v2, p0, La/a/a/d/KC_e;->b:I

    if-le v2, v0, :cond_2

    iget v0, p0, La/a/a/d/KC_e;->b:I

    iget v2, p0, La/a/a/d/KC_e;->b:I

    shr-int/lit8 v2, v2, 0x1

    add-int/2addr v0, v2

    .line 472
    :cond_2
    iget-object v2, p0, La/a/a/d/KC_e;->g:[I

    .line 473
    iget-object v3, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    .line 474
    invoke-direct {p0, v0}, La/a/a/d/KC_e;->d(I)V

    .line 476
    iget v0, p0, La/a/a/d/KC_e;->b:I

    add-int/lit8 v0, v0, -0x1

    iput v0, p0, La/a/a/d/KC_e;->b:I

    .line 477
    if-lez p1, :cond_3

    .line 479
    iget-object v0, p0, La/a/a/d/KC_e;->g:[I

    invoke-static {v2, v5, v0, v5, p1}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    .line 480
    iget-object v0, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    shl-int/lit8 v4, p1, 0x1

    invoke-static {v3, v5, v0, v5, v4}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    .line 482
    :cond_3
    iget v0, p0, La/a/a/d/KC_e;->b:I

    if-ge p1, v0, :cond_0

    .line 485
    add-int/lit8 v0, p1, 0x1

    iget-object v4, p0, La/a/a/d/KC_e;->g:[I

    iget v5, p0, La/a/a/d/KC_e;->b:I

    sub-int/2addr v5, p1

    invoke-static {v2, v0, v4, p1, v5}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    .line 486
    add-int/lit8 v0, p1, 0x1

    shl-int/lit8 v0, v0, 0x1

    iget-object v2, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    shl-int/lit8 v4, p1, 0x1

    iget v5, p0, La/a/a/d/KC_e;->b:I

    sub-int/2addr v5, p1

    shl-int/lit8 v5, v5, 0x1

    invoke-static {v3, v0, v2, v4, v5}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    goto :goto_0

    .line 490
    :cond_4
    iget v0, p0, La/a/a/d/KC_e;->b:I

    add-int/lit8 v0, v0, -0x1

    iput v0, p0, La/a/a/d/KC_e;->b:I

    .line 491
    iget v0, p0, La/a/a/d/KC_e;->b:I

    if-ge p1, v0, :cond_5

    .line 494
    iget-object v0, p0, La/a/a/d/KC_e;->g:[I

    add-int/lit8 v2, p1, 0x1

    iget-object v3, p0, La/a/a/d/KC_e;->g:[I

    iget v4, p0, La/a/a/d/KC_e;->b:I

    sub-int/2addr v4, p1

    invoke-static {v0, v2, v3, p1, v4}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    .line 495
    iget-object v0, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    add-int/lit8 v2, p1, 0x1

    shl-int/lit8 v2, v2, 0x1

    iget-object v3, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    shl-int/lit8 v4, p1, 0x1

    iget v5, p0, La/a/a/d/KC_e;->b:I

    sub-int/2addr v5, p1

    shl-int/lit8 v5, v5, 0x1

    invoke-static {v0, v2, v3, v4, v5}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    .line 498
    :cond_5
    iget-object v0, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    iget v2, p0, La/a/a/d/KC_e;->b:I

    shl-int/lit8 v2, v2, 0x1

    aput-object v6, v0, v2

    .line 499
    iget-object v0, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    iget v2, p0, La/a/a/d/KC_e;->b:I

    shl-int/lit8 v2, v2, 0x1

    add-int/lit8 v2, v2, 0x1

    aput-object v6, v0, v2

    goto/16 :goto_0
.end method

.method public clear()V
    .locals 3

    .prologue
    .line 241
    iget v0, p0, La/a/a/d/KC_e;->b:I

    if-eqz v0, :cond_0

    .line 242
    iget-object v0, p0, La/a/a/d/KC_e;->g:[I

    iget-object v1, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    iget v2, p0, La/a/a/d/KC_e;->b:I

    invoke-static {v0, v1, v2}, La/a/a/d/KC_e;->a([I[Ljava/lang/Object;I)V

    .line 243
    sget-object v0, La/a/a/d/KC_b;->a:[I

    iput-object v0, p0, La/a/a/d/KC_e;->g:[I

    .line 244
    sget-object v0, La/a/a/d/KC_b;->b:[Ljava/lang/Object;

    iput-object v0, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    .line 245
    const/4 v0, 0x0

    iput v0, p0, La/a/a/d/KC_e;->b:I

    .line 247
    :cond_0
    return-void
.end method

.method public containsKey(Ljava/lang/Object;)Z
    .locals 3
    .param p1, "key"    # Ljava/lang/Object;

    .prologue
    .local p0, "this":La/a/a/d/KC_e;, "La/a/a/d/KC_e<TK;TV;>;"
    const/4 v0, 0x1

    const/4 v1, 0x0

    .line 273
    if-nez p1, :cond_2

    invoke-virtual {p0}, La/a/a/d/KC_e;->a()I

    move-result v2

    if-ltz v2, :cond_1

    :cond_0
    :goto_0
    return v0

    :cond_1
    move v0, v1

    goto :goto_0

    :cond_2
    invoke-virtual {p1}, Ljava/lang/Object;->hashCode()I

    move-result v2

    invoke-virtual {p0, p1, v2}, La/a/a/d/KC_e;->a(Ljava/lang/Object;I)I

    move-result v2

    if-gez v2, :cond_0

    move v0, v1

    goto :goto_0
.end method

.method public containsValue(Ljava/lang/Object;)Z
    .locals 1
    .param p1, "value"    # Ljava/lang/Object;

    .prologue
    .line 303
    .local p0, "this":La/a/a/d/KC_e;, "La/a/a/d/KC_e<TK;TV;>;"
    invoke-virtual {p0, p1}, La/a/a/d/KC_e;->a(Ljava/lang/Object;)I

    move-result v0

    if-ltz v0, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public equals(Ljava/lang/Object;)Z
    .locals 6
    .param p1, "object"    # Ljava/lang/Object;

    .prologue
    .local p0, "this":La/a/a/d/KC_e;, "La/a/a/d/KC_e<TK;TV;>;"
    const/4 v0, 0x1

    const/4 v1, 0x0

    .line 522
    if-ne p0, p1, :cond_1

    .line 551
    .end local p1    # "object":Ljava/lang/Object;
    :cond_0
    :goto_0
    return v0

    .line 525
    .restart local p1    # "object":Ljava/lang/Object;
    :cond_1
    instance-of v2, p1, Ljava/util/Map;

    if-eqz v2, :cond_6

    .line 526
    check-cast p1, Ljava/util/Map;

    .line 527
    .end local p1    # "object":Ljava/lang/Object;
    invoke-virtual {p0}, La/a/a/d/KC_e;->size()I

    move-result v2

    invoke-interface {p1}, Ljava/util/Map;->size()I

    move-result v3

    if-eq v2, v3, :cond_2

    move v0, v1

    .line 528
    goto :goto_0

    :cond_2
    move v2, v1

    .line 532
    :goto_1
    :try_start_0
    iget v3, p0, La/a/a/d/KC_e;->b:I

    if-ge v2, v3, :cond_0

    .line 533
    invoke-direct {p0, v2}, La/a/a/d/KC_e;->e(I)Ljava/lang/Object;

    move-result-object v3

    .line 534
    invoke-virtual {p0, v2}, La/a/a/d/KC_e;->b(I)Ljava/lang/Object;

    move-result-object v4

    .line 535
    invoke-interface {p1, v3}, Ljava/util/Map;->get(Ljava/lang/Object;)Ljava/lang/Object;

    move-result-object v5

    .line 536
    if-nez v4, :cond_4

    .line 537
    if-nez v5, :cond_3

    invoke-interface {p1, v3}, Ljava/util/Map;->containsKey(Ljava/lang/Object;)Z

    move-result v3

    if-nez v3, :cond_5

    :cond_3
    move v0, v1

    .line 538
    goto :goto_0

    .line 540
    :cond_4
    invoke-virtual {v4, v5}, Ljava/lang/Object;->equals(Ljava/lang/Object;)Z
    :try_end_0
    .catch Ljava/lang/NullPointerException; {:try_start_0 .. :try_end_0} :catch_0
    .catch Ljava/lang/ClassCastException; {:try_start_0 .. :try_end_0} :catch_1

    move-result v3

    if-nez v3, :cond_5

    move v0, v1

    .line 541
    goto :goto_0

    .line 532
    :cond_5
    add-int/lit8 v2, v2, 0x1

    goto :goto_1

    .line 545
    :catch_0
    move-exception v0

    move v0, v1

    goto :goto_0

    .line 547
    :catch_1
    move-exception v0

    move v0, v1

    goto :goto_0

    .restart local p1    # "object":Ljava/lang/Object;
    :cond_6
    move v0, v1

    .line 551
    goto :goto_0
.end method

.method public get(Ljava/lang/Object;)Ljava/lang/Object;
    .locals 2
    .param p1, "key"    # Ljava/lang/Object;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Ljava/lang/Object;",
            ")TV;"
        }
    .end annotation

    .prologue
    .line 313
    .local p0, "this":La/a/a/d/KC_e;, "La/a/a/d/KC_e<TK;TV;>;"
    if-nez p1, :cond_0

    invoke-virtual {p0}, La/a/a/d/KC_e;->a()I

    move-result v0

    .line 314
    :goto_0
    if-ltz v0, :cond_1

    iget-object v1, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    shl-int/lit8 v0, v0, 0x1

    add-int/lit8 v0, v0, 0x1

    aget-object v0, v1, v0

    :goto_1
    return-object v0

    .line 313
    :cond_0
    invoke-virtual {p1}, Ljava/lang/Object;->hashCode()I

    move-result v0

    invoke-virtual {p0, p1, v0}, La/a/a/d/KC_e;->a(Ljava/lang/Object;I)I

    move-result v0

    goto :goto_0

    .line 314
    :cond_1
    const/4 v0, 0x0

    goto :goto_1
.end method

.method public hashCode()I
    .locals 9

    .prologue
    const/4 v1, 0x0

    .line 559
    iget-object v5, p0, La/a/a/d/KC_e;->g:[I

    .line 560
    iget-object v6, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    .line 562
    const/4 v0, 0x1

    iget v7, p0, La/a/a/d/KC_e;->b:I

    move v2, v0

    move v3, v1

    move v4, v1

    :goto_0
    if-ge v3, v7, :cond_1

    .line 563
    aget-object v0, v6, v2

    .line 564
    aget v8, v5, v3

    if-nez v0, :cond_0

    move v0, v1

    :goto_1
    xor-int/2addr v0, v8

    add-int/2addr v4, v0

    .line 562
    add-int/lit8 v3, v3, 0x1

    add-int/lit8 v0, v2, 0x2

    move v2, v0

    goto :goto_0

    .line 564
    :cond_0
    invoke-virtual {v0}, Ljava/lang/Object;->hashCode()I

    move-result v0

    goto :goto_1

    .line 566
    :cond_1
    return v4
.end method

.method public isEmpty()Z
    .locals 1

    .prologue
    .line 352
    iget v0, p0, La/a/a/d/KC_e;->b:I

    if-gtz v0, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public put(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;
    .locals 7
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(TK;TV;)TV;"
        }
    .end annotation

    .prologue
    .local p0, "this":La/a/a/d/KC_e;, "La/a/a/d/KC_e<TK;TV;>;"
    .local p1, "key":Ljava/lang/Object;, "TK;"
    .local p2, "value":Ljava/lang/Object;, "TV;"
    const/16 v0, 0x8

    const/4 v1, 0x4

    const/4 v4, 0x0

    .line 366
    if-nez p1, :cond_0

    .line 368
    invoke-virtual {p0}, La/a/a/d/KC_e;->a()I

    move-result v2

    move v3, v4

    .line 373
    :goto_0
    if-ltz v2, :cond_1

    .line 374
    shl-int/lit8 v0, v2, 0x1

    add-int/lit8 v1, v0, 0x1

    .line 375
    iget-object v0, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    aget-object v0, v0, v1

    .line 376
    iget-object v2, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    aput-object p2, v2, v1

    .line 411
    :goto_1
    return-object v0

    .line 370
    :cond_0
    invoke-virtual {p1}, Ljava/lang/Object;->hashCode()I

    move-result v3

    .line 371
    invoke-virtual {p0, p1, v3}, La/a/a/d/KC_e;->a(Ljava/lang/Object;I)I

    move-result v2

    goto :goto_0

    .line 380
    :cond_1
    xor-int/lit8 v2, v2, -0x1

    .line 381
    iget v5, p0, La/a/a/d/KC_e;->b:I

    iget-object v6, p0, La/a/a/d/KC_e;->g:[I

    array-length v6, v6

    if-lt v5, v6, :cond_4

    .line 382
    iget v5, p0, La/a/a/d/KC_e;->b:I

    if-lt v5, v0, :cond_6

    iget v0, p0, La/a/a/d/KC_e;->b:I

    iget v1, p0, La/a/a/d/KC_e;->b:I

    shr-int/lit8 v1, v1, 0x1

    add-int/2addr v0, v1

    .line 387
    :cond_2
    :goto_2
    iget-object v1, p0, La/a/a/d/KC_e;->g:[I

    .line 388
    iget-object v5, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    .line 389
    invoke-direct {p0, v0}, La/a/a/d/KC_e;->d(I)V

    .line 391
    iget-object v0, p0, La/a/a/d/KC_e;->g:[I

    array-length v0, v0

    if-lez v0, :cond_3

    .line 393
    iget-object v0, p0, La/a/a/d/KC_e;->g:[I

    array-length v6, v1

    invoke-static {v1, v4, v0, v4, v6}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    .line 394
    iget-object v0, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    array-length v6, v5

    invoke-static {v5, v4, v0, v4, v6}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    .line 397
    :cond_3
    iget v0, p0, La/a/a/d/KC_e;->b:I

    invoke-static {v1, v5, v0}, La/a/a/d/KC_e;->a([I[Ljava/lang/Object;I)V

    .line 400
    :cond_4
    iget v0, p0, La/a/a/d/KC_e;->b:I

    if-ge v2, v0, :cond_5

    .line 403
    iget-object v0, p0, La/a/a/d/KC_e;->g:[I

    iget-object v1, p0, La/a/a/d/KC_e;->g:[I

    add-int/lit8 v4, v2, 0x1

    iget v5, p0, La/a/a/d/KC_e;->b:I

    sub-int/2addr v5, v2

    invoke-static {v0, v2, v1, v4, v5}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    .line 404
    iget-object v0, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    shl-int/lit8 v1, v2, 0x1

    iget-object v4, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    add-int/lit8 v5, v2, 0x1

    shl-int/lit8 v5, v5, 0x1

    iget v6, p0, La/a/a/d/KC_e;->b:I

    sub-int/2addr v6, v2

    shl-int/lit8 v6, v6, 0x1

    invoke-static {v0, v1, v4, v5, v6}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    .line 407
    :cond_5
    iget-object v0, p0, La/a/a/d/KC_e;->g:[I

    aput v3, v0, v2

    .line 408
    iget-object v0, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    shl-int/lit8 v1, v2, 0x1

    aput-object p1, v0, v1

    .line 409
    iget-object v0, p0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    shl-int/lit8 v1, v2, 0x1

    add-int/lit8 v1, v1, 0x1

    aput-object p2, v0, v1

    .line 410
    iget v0, p0, La/a/a/d/KC_e;->b:I

    add-int/lit8 v0, v0, 0x1

    iput v0, p0, La/a/a/d/KC_e;->b:I

    .line 411
    const/4 v0, 0x0

    goto :goto_1

    .line 382
    :cond_6
    iget v5, p0, La/a/a/d/KC_e;->b:I

    if-ge v5, v1, :cond_2

    move v0, v1

    goto :goto_2
.end method

.method public remove(Ljava/lang/Object;)Ljava/lang/Object;
    .locals 1
    .param p1, "key"    # Ljava/lang/Object;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Ljava/lang/Object;",
            ")TV;"
        }
    .end annotation

    .prologue
    .line 441
    .local p0, "this":La/a/a/d/KC_e;, "La/a/a/d/KC_e<TK;TV;>;"
    if-nez p1, :cond_0

    invoke-virtual {p0}, La/a/a/d/KC_e;->a()I

    move-result v0

    .line 442
    :goto_0
    if-ltz v0, :cond_1

    .line 443
    invoke-virtual {p0, v0}, La/a/a/d/KC_e;->c(I)Ljava/lang/Object;

    move-result-object v0

    .line 446
    :goto_1
    return-object v0

    .line 441
    :cond_0
    invoke-virtual {p1}, Ljava/lang/Object;->hashCode()I

    move-result v0

    invoke-virtual {p0, p1, v0}, La/a/a/d/KC_e;->a(Ljava/lang/Object;I)I

    move-result v0

    goto :goto_0

    .line 446
    :cond_1
    const/4 v0, 0x0

    goto :goto_1
.end method

.method public size()I
    .locals 1

    .prologue
    .line 509
    iget v0, p0, La/a/a/d/KC_e;->b:I

    return v0
.end method

.method public toString()Ljava/lang/String;
    .locals 3

    .prologue
    .line 578
    invoke-virtual {p0}, La/a/a/d/KC_e;->isEmpty()Z

    move-result v0

    if-eqz v0, :cond_0

    .line 579
    const-string v0, "{}"

    .line 603
    :goto_0
    return-object v0

    .line 582
    :cond_0
    new-instance v1, Ljava/lang/StringBuilder;

    iget v0, p0, La/a/a/d/KC_e;->b:I

    mul-int/lit8 v0, v0, 0x1c

    invoke-direct {v1, v0}, Ljava/lang/StringBuilder;-><init>(I)V

    .line 583
    const/16 v0, 0x7b

    invoke-virtual {v1, v0}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 584
    const/4 v0, 0x0

    :goto_1
    iget v2, p0, La/a/a/d/KC_e;->b:I

    if-ge v0, v2, :cond_4

    .line 585
    if-lez v0, :cond_1

    .line 586
    const-string v2, ", "

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 588
    :cond_1
    invoke-direct {p0, v0}, La/a/a/d/KC_e;->e(I)Ljava/lang/Object;

    move-result-object v2

    .line 589
    if-eq v2, p0, :cond_2

    .line 590
    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    .line 594
    :goto_2
    const/16 v2, 0x3d

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 595
    invoke-virtual {p0, v0}, La/a/a/d/KC_e;->b(I)Ljava/lang/Object;

    move-result-object v2

    .line 596
    if-eq v2, p0, :cond_3

    .line 597
    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    .line 584
    :goto_3
    add-int/lit8 v0, v0, 0x1

    goto :goto_1

    .line 592
    :cond_2
    const-string v2, "(this Map)"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    goto :goto_2

    .line 599
    :cond_3
    const-string v2, "(this Map)"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    goto :goto_3

    .line 602
    :cond_4
    const/16 v0, 0x7d

    invoke-virtual {v1, v0}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 603
    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    goto :goto_0
.end method
